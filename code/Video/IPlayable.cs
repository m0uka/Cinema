namespace Cinema.Video
{
	public interface IPlayable
	{
		void Play(string id);
		void RequestVideo( string url );
	}
}
