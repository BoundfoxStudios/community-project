using System;
using System.Linq;
using BoundfoxStudios.CommunityProject.Navigation.PathProviders;
using BoundfoxStudios.CommunityProject.Tests.Editor.Extensions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using UnityEngine.Splines;

namespace BoundfoxStudios.CommunityProject.Tests.Editor.Navigation.PathProviders
{
	/*
	 Notice: We're not using mocks of ISpline here, otherwise we'd need to mock an enumerator to make the tests work.
	 Also notice: This tests are currently super confusing to read because everything in the Splines package
								works on indices. Additionally, when trying to make a decision on a link which splines to use,
								the SplinePathProvider only provides splines that make sense to follow. By that, the indices
								of the SplineKnotIndex will change.
	*/
	public class SplinePathProviderTests
	{
		[Test]
		public void CanSuccessfullyCreateAPathOnASingleSpline()
		{
			var splineContainerMock = CreateSplineContainerMock(
				new[]
				{
					new Spline(new[]
					{
						new BezierKnot(new(0, 0, 0)),
						new BezierKnot(new(0, 0, 1)),
						new BezierKnot(new(0, 0, 2)),
						new BezierKnot(new(0, 0, 3))
					})
				},
				new()
			);

			var splineLinkDecider = new RandomSplineLinkDecisionMaker();
			var sut = new SplinePathProvider();

			Action action = () => sut.CreatePath(splineContainerMock.Object, splineLinkDecider);

			action.Should().NotThrow();

			var result = sut.CreatePath(splineContainerMock.Object, splineLinkDecider);

			result.Count.Should().Be(4);

			var knots = result.ToArray();
			knots[0].ShouldBeAtPosition(new(0, 0, 0));
			knots[1].ShouldBeAtPosition(new(0, 0, 1));
			knots[2].ShouldBeAtPosition(new(0, 0, 2));
			knots[3].ShouldBeAtPosition(new(0, 0, 3));
		}

		[Test]
		public void CanSuccessfullyCreateAPathOfTwoSplinesThatSplitTheMainSplineInTheMiddleAndMergeCorrectly()
		{
			var splitKnot = new BezierKnot(new(0, 0, 1));
			var mergeKnot = new BezierKnot(new(0, 0, 2));

			var mainSpline = new Spline(new[]
			{
				new BezierKnot(new(0, 0, 0)),
				splitKnot,
				mergeKnot,
				new BezierKnot(new(0, 0, 3))
			});

			var sideSpline = new Spline(new[]
			{
				splitKnot,
				new BezierKnot(new(1, 0, 1)),
				new BezierKnot(new(2, 0, 1)),
				mergeKnot
			});

			var knotLinks = new KnotLinkCollection();
			knotLinks.Link(
				new(0, 1),
				new(1, 0)
			);
			knotLinks.Link(
				new(0, 2),
				new(1, 3)
			);

			var splineContainerMock = CreateSplineContainerMock(
				new[]
				{
					mainSpline,
					sideSpline
				},
				knotLinks
			);

			var sut = new SplinePathProvider();

			var splineLinkDecider = new SequenceSplineLinkDecisionMaker(1, 0);

			Action action = () => sut.CreatePath(splineContainerMock.Object, splineLinkDecider);

			action.Should().NotThrow();

			splineLinkDecider.Reset();
			var result = sut.CreatePath(splineContainerMock.Object, splineLinkDecider);

			result[^1].ShouldBeAtPosition(new(0, 0, 3));
		}

		[Test]
		public void CanSuccessfullyCreateAPathOfTwoSplinesThatSplitTheMainSplineAtStartAndMergeAtTheEndCorrectly()
		{
			var splitKnot = new BezierKnot(new(0, 0, 0));
			var mergeKnot = new BezierKnot(new(0, 0, 3));

			var mainSpline = new Spline(new[]
			{
				splitKnot,
				new BezierKnot(new(0, 0, 1)),
				new BezierKnot(new(0, 0, 2)),
				mergeKnot
			});

			var sideSpline = new Spline(new[]
			{
				splitKnot,
				new BezierKnot(new(1, 0, 1)),
				new BezierKnot(new(2, 0, 1)),
				mergeKnot
			});

			var knotLinks = new KnotLinkCollection();
			knotLinks.Link(
				new(0, 0),
				new(1, 0)
			);
			knotLinks.Link(
				new(0, 3),
				new(1, 3)
			);

			var splineContainerMock = CreateSplineContainerMock(
				new[]
				{
					mainSpline,
					sideSpline
				},
				knotLinks
			);

			var sut = new SplinePathProvider();

			var splineLinkDecider = new SequenceSplineLinkDecisionMaker(1, 0);

			Action action = () => sut.CreatePath(splineContainerMock.Object, splineLinkDecider);

			action.Should().NotThrow();

			splineLinkDecider.Reset();
			var result = sut.CreatePath(splineContainerMock.Object, splineLinkDecider);

			result[^1].ShouldBeAtPosition(new(0, 0, 3));
		}

		[Test]
		public void WillFailIfASplineSplitsButNeverMergesBackToTheMainSpline()
		{
			var splitKnot = new BezierKnot(new(0, 0, 1));

			var mainSpline = new Spline(new[]
			{
				new BezierKnot(new(0, 0, 0)),
				splitKnot,
				new BezierKnot(new(0, 0, 2)),
				new BezierKnot(new(0, 0, 3))
			});

			var sideSpline = new Spline(new[]
			{
				splitKnot,
				new BezierKnot(new(1, 0, 1)),
				new BezierKnot(new(2, 0, 1)),
				new BezierKnot(new(3, 0, 1))
			});

			var knotLinks = new KnotLinkCollection();
			knotLinks.Link(
				new(0, 1),
				new(1, 0)
			);

			var splineContainerMock = CreateSplineContainerMock(
				new[]
				{
					mainSpline,
					sideSpline
				},
				knotLinks
			);

			var sut = new SplinePathProvider();

			var splineLinkDecider = new SequenceSplineLinkDecisionMaker(1);

			Action action = () => sut.CreatePath(splineContainerMock.Object, splineLinkDecider);

			action.Should().Throw<Exception>().WithMessage("Did not find a valid path*");
		}


		/**
		 * The following splines will be created here:
		 *
		 * 3.            /--------\
		 * 2.      /---(2)--------(3)---------\
		 * 1.  --(1)--------------------------(4)--
		 * 4.     \---------------------------/
		 *
		 * x.: denotes the spline index
		 * (y): denotes the knot link
		 */
		[Test]
		[TestCase(new[] { 0, 0 })] // 1. only
		[TestCase(new[] { 1, 0, 0, 0 })] // 1. -> 2. -> 1.
		[TestCase(new[] { 2, 0 })] // 1. -> 4. -> 1
		[TestCase(new[] { 1, 1, 0, 0 })] // 1. -> 2. -> 3. -> 1.
		public void CanSuccessfullyCreateAPath(int[] linkDecisions)
		{
			var firstLinkKnot = new BezierKnot(0, 0, 1);
			var secondLinkKnot = new BezierKnot(1, 0, 2);
			var thirdLinkKnot = new BezierKnot(2, 0, 2);
			var fourthLinkKnot = new BezierKnot(0, 0, 3);

			var mainSpline = new Spline(new[]
			{
				new BezierKnot(new(0, 0, 0)),
				firstLinkKnot,
				fourthLinkKnot,
				new BezierKnot(new(0, 0, 3))
			});

			var secondSpline = new Spline(new[]
			{
				firstLinkKnot,
				secondLinkKnot,
				thirdLinkKnot,
				fourthLinkKnot
			});

			var thirdSpline = new Spline(new[]
			{
				secondLinkKnot,
				new BezierKnot(new(1, 1, 2)),
				new BezierKnot(new(1, 2, 2)),
				thirdLinkKnot
			});

			var fourthSpline = new Spline(new[]
			{
				firstLinkKnot,
				new BezierKnot(new(-1, 0, 1)),
				new BezierKnot(new(-2, 0, 1)),
				fourthLinkKnot
			});

			var knotLinks = new KnotLinkCollection();

			// (1) 1./2.
			knotLinks.Link(
				new(0, 1),
				new(1, 0)
			);

			// (1) 1./4.
			knotLinks.Link(
				new(0, 1),
				new(3, 0)
			);

			// (2) 2./3.
			knotLinks.Link(
				new(1, 1),
				new(2, 0)
			);

			// (3) 3./2.
			knotLinks.Link(
				new(2, 3),
				new(1, 2)
			);

			// (4) 2./1.
			knotLinks.Link(
				new(1, 3),
				new(0, 2)
			);

			// (4) 4./1.
			knotLinks.Link(
				new(3, 3),
				new(0, 2)
			);

			var splineContainerMock = CreateSplineContainerMock(
				new[]
				{
					mainSpline,
					secondSpline,
					thirdSpline,
					fourthSpline
				},
				knotLinks
			);

			var sut = new SplinePathProvider();

			var splineLinkDecider = new SequenceSplineLinkDecisionMaker(linkDecisions);

			Action action = () => sut.CreatePath(splineContainerMock.Object, splineLinkDecider);

			action.Should().NotThrow();

			splineLinkDecider.Reset();
			var result = sut.CreatePath(splineContainerMock.Object, splineLinkDecider);

			result[^1].ShouldBeAtPosition(new(0, 0, 3));
		}

		private Mock<ISplineContainer> CreateSplineContainerMock(Spline[] splines, KnotLinkCollection knotLinkCollection)
		{
			var splineContainerMock = new Mock<ISplineContainer>();

			splineContainerMock.SetupGet(container => container.Splines).Returns(splines);
			splineContainerMock.SetupGet(container => container.KnotLinkCollection).Returns(knotLinkCollection);

			return splineContainerMock;
		}

		/// <summary>
		///   Test class to take a predefined route by providing the candidate index that the
		///   <see cref="ISplineLinkDecisionMaker" /> will resolve to.
		/// </summary>
		private class SequenceSplineLinkDecisionMaker : ISplineLinkDecisionMaker
		{
			private readonly int[] _candidateIndices;
			private int _currentCandidateIndex;

			/// <param name="candidateIndices">
			///   List of indices in the <see cref="KnotLinkCollection" /> that are used to determine the
			///   candidates in <see cref="Decide" />.
			/// </param>
			public SequenceSplineLinkDecisionMaker(params int[] candidateIndices) => _candidateIndices = candidateIndices;

			public SplineKnotIndex Decide(SplineKnotIndex[] candidates) =>
				candidates[_candidateIndices[_currentCandidateIndex++]];

			public void Reset() => _currentCandidateIndex = 0;
		}
	}
}
