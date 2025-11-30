using R3;

namespace Modules.Toolbag.Extensions
{
	public static class SubjectExtensions
	{
		public static void Emit<T>(this Subject<T> subject, T value)
		{
			subject.OnNext(value);
		}
		
		public static void Emit(this Subject<Unit> subject)
		{
			subject.OnNext(Unit.Default);
		}
	}
}