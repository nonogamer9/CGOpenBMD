using System;
using UnityEngine;

namespace DG.DemiLib
{
	public abstract class DeScope : IDisposable
	{
		private bool _disposed;

		protected abstract void CloseScope();

		~DeScope()
		{
			if (!_disposed)
			{
				Debug.LogError("Scope was not disposed! You should use the 'using' keyword or manually call Dispose.");
				Dispose();
			}
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				CloseScope();
			}
		}
	}
}
