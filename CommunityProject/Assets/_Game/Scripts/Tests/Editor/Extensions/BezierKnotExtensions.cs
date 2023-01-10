using FluentAssertions;
using Unity.Mathematics;
using UnityEngine.Splines;

namespace BoundfoxStudios.CommunityProject.Tests.Editor.Extensions
{
	public static class BezierKnotExtensions
	{
		public static void ShouldBeAtPosition(this BezierKnot knot, float3 position)
		{
			// Little helpers to avoid boxing.
			var knotPosition = knot.Position;
			knotPosition.x.Should().Be(position.x);
			knotPosition.y.Should().Be(position.y);
			knotPosition.z.Should().Be(position.z);
		}
	}
}
