using UnityEngine.Splines;

namespace BoundfoxStudios.CommunityProject.Extensions
{
	public static class SplineExtensions
	{
		public static bool IsValidKnotIndex(this ISpline spline, int knotIndex) => knotIndex >= 0 && knotIndex < spline.Count;
	}
}
