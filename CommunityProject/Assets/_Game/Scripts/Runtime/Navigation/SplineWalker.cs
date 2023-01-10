using BoundfoxStudios.CommunityProject.Navigation.PathProviders;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace BoundfoxStudios.CommunityProject.Navigation
{
	[AddComponentMenu(Constants.MenuNames.Navigation + "/" + nameof(SplineWalker))]
	public class SplineWalker : MonoBehaviour
	{
		[SerializeField]
		private Rigidbody Rigidbody;

		// TODO: Later, the Spline will be set by the WaveSpawner
		private ISpline _spline;
		public SplineContainer Container;

		// TODO: This will later be set by an SO
		public float Speed = 1;

		/// <summary>
		/// Overall duration it will take to traverse <see cref="_spline"/>
		/// </summary>
		private float _duration;

		/// <summary>
		/// 0..1 where 0 is the start of the spline and 1 is the end of the spline.
		/// </summary>
		private float _normalizedTime;

		private float _elapsedTime;

		private void Awake()
		{
			var pathProvider = new SplinePathProvider();
			_spline = pathProvider.CreatePath(Container, new RandomSplineLinkDecisionMaker());
			_duration = _spline.GetLength() / Speed;
		}

		private void CalculateNormalizedTime(float deltaTime)
		{
			_elapsedTime += deltaTime;

			var t = math.min(_elapsedTime, _duration);
			t /= _duration;

			_normalizedTime = math.floor(_normalizedTime) + t;
		}

		private void FixedUpdate()
		{
			CalculateNormalizedTime(Time.deltaTime);

			Container.Evaluate(_spline, _normalizedTime, out var position, out var tangent, out _);

			var rotation = Quaternion.LookRotation(tangent);

			Rigidbody.Move(position, rotation);

			if (_normalizedTime > 1)
			{
				Destroy(gameObject);
			}
		}
	}
}
