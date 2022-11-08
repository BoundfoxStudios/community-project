using BoundfoxStudios.CommunityProject.Build.BuildManifest;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoundfoxStudios.CommunityProject.UI
{
	[AddComponentMenu(Constants.MenuNames.UI + "/" + nameof(ViewBuildNumber))]
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class ViewBuildNumber : MonoBehaviour, IPointerClickHandler
	{
		private TextMeshProUGUI _buildNumberText;

		public void OnPointerClick(PointerEventData eventData)
		{
			CopyBuildNumberToClipboard();
		}

		[UsedImplicitly]
		// ReSharper disable once Unity.IncorrectMethodSignature
		private async UniTaskVoid Awake()
		{
			_buildNumberText = gameObject.GetComponent<TextMeshProUGUI>();
			_buildNumberText.text = await CreateBuildNumberAsync();
		}

		private async UniTask<string> CreateBuildNumberAsync()
		{
			var buildManifestReader = new BuildManifestReader();
			var buildManifest = await buildManifestReader.LoadAsync();
			return $"Build: {Application.version} ({buildManifest.ShortSha})";
		}

		private void CopyBuildNumberToClipboard()
		{
			GUIUtility.systemCopyBuffer = _buildNumberText.text;
		}
	}
}
