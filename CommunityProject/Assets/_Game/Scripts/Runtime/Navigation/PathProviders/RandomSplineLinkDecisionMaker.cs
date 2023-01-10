using UnityEngine;
using UnityEngine.Splines;

namespace BoundfoxStudios.CommunityProject.Navigation.PathProviders
{
	public class RandomSplineLinkDecisionMaker : ISplineLinkDecisionMaker
	{
		public SplineKnotIndex Decide(SplineKnotIndex[] candidates) => candidates[Random.Range(0, candidates.Length)];
	}
}
