using System;

namespace UnityEngine.Purchasing
{
	internal interface IAsyncWebUtil
	{
		void Get(string url, Action<string> responseHandler, Action<string> errorHandler, int maxTimeoutInSeconds = 30);

		void Post(string url, string body, Action<string> responseHandler, Action<string> errorHandler, int maxTimeoutInSeconds = 30);

		void Schedule(Action a, int delayInSeconds);
	}
}
