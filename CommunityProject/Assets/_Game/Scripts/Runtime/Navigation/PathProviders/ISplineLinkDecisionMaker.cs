using UnityEngine.Splines;

namespace BoundfoxStudios.CommunityProject.Navigation.PathProviders
{
	public interface ISplineLinkDecisionMaker
	{
		SplineKnotIndex Decide(SplineKnotIndex[] candidates);
	}
}
