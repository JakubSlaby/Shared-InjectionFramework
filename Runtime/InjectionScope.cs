using System;

namespace WhiteSparrow.Shared.DependencyInjection
{
	public static class InjectionScope
	{
		public abstract class Scope : IDisposable
		{
			private bool m_Disposed;
			
			~Scope()
			{
				this.Dispose();
			}
			
			public void Dispose()
			{
				_Dispose();
				GC.SuppressFinalize(this);
			}

			private void _Dispose()
			{
				if (m_Disposed)
					return;
				
				CloseScope();
				m_Disposed = true;
			}

			protected abstract void CloseScope();
		}
		
		public class DisallowMapping : Scope
		{
			protected override void CloseScope()
			{
				
			}
		}

		public class Context : Scope
		{
			protected override void CloseScope()
			{
				
			}
		}
	}
}