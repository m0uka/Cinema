using Cinema.Entities;
using Cinema.Video;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Cinema.UI
{
	public class VideoRequestPanel : Panel
	{
		public static VideoRequestPanel Instance { get; set; }
		
		public Label InfoLabel { get; set; }
		public TextEntry UrlInput { get; set; }
		public Button SubmitButton { get; set; }
		
		public TVEntity Playable { get; set; }

		public VideoRequestPanel()
		{
			InfoLabel = Add.Label( "Video URL", "label" );
			UrlInput = Add.TextEntry("");
			UrlInput.AddClass("textentry");

			SubmitButton = Add.Button("Play", () =>
			{
				// Playable.RequestVideo(UrlInput.StringValue);
				Style.Display = DisplayMode.None;
				Style.PointerEvents = "none";
				Style.Dirty();

				ConsoleSystem.Run( "request_video", Playable.NetworkIdent, UrlInput.Text );
			});
			SubmitButton.AddClass( "button" );
			
			StyleSheet.Load( "UI/VideoRequestPanel.scss" );

			Instance = this;
		}

		public void SetPlayer( TVEntity playable )
		{
			Playable = playable;
			Style.Display = DisplayMode.Flex;
			Style.PointerEvents = "all";
			Style.Dirty();
		}
	}
}
