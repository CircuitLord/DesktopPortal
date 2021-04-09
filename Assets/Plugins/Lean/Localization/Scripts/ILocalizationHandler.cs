namespace Lean.Localization
{
	public interface ILocalizationHandler
	{
		void UpdateLocalization();

		void Register(LeanToken token);
		void Unregister(LeanToken token);
		void UnregisterAll();
	}
}