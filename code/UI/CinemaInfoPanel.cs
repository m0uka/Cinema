using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Cinema.UI;

public class CinemaInfoPanel : Panel
{
	public Label Text { get; set; }

	public CinemaInfoPanel()
	{
		Text = Add.Label(
			string.Join("\n", 
				"Cinema",
				"This is a proof of concept cinema for s&box.",
				"Press E to request a video, only YouTube links are supported!",
				"Video might be out of sync with sound and multiplayer video sync is not guaranteed.")
		);

		InitPanel();;
	}

	[Event.HotloadAttribute]
	public void InitPanel()
	{
		Text.Style.FontSize = Length.Pixels(16);
		Text.Style.Margin = Length.Pixels( 20 );
		Text.Style.FontColor = Color.White;
		Text.Style.TextAlign = TextAlign.Left;
		Text.Style.AlignItems = Align.FlexEnd;
	}
}
