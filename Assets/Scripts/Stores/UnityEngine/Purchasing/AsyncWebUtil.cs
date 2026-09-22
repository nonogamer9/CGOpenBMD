using System;
using System.Collections;
using System.Text;

namespace UnityEngine.Purchasing
{
	[AddComponentMenu("")]
	internal class AsyncWebUtil : MonoBehaviour, IAsyncWebUtil
	{
		public void Get(string url, Action<string> responseHandler, Action<string> errorHandler, int maxTimeoutInSeconds = 5)
		{
			WWW request = new WWW(url);
			StartCoroutine(Process(request, responseHandler, errorHandler, maxTimeoutInSeconds));
		}

		public void Post(string url, string body, Action<string> responseHandler, Action<string> errorHandler, int maxTimeoutInSeconds = 5)
		{
			Encoding uTF = Encoding.UTF8;
			WWW request = new WWW(url, uTF.GetBytes(body));
			StartCoroutine(Process(request, responseHandler, errorHandler, maxTimeoutInSeconds));
		}

		public void Schedule(Action a, int delayInSeconds)
		{
			StartCoroutine(DoInvoke(a, delayInSeconds));
		}

		private IEnumerator DoInvoke(Action a, int delayInSeconds)
		{
			yield return new WaitForSeconds(delayInSeconds);
			a();
		}

		private IEnumerator Process(WWW request, Action<string> responseHandler, Action<string> errorHandler, int maxTimeoutInSeconds)
		{
			float timer = 0f;
			bool hasTimedOut = false;
			while (!request.isDone)
			{
				if (timer > (float)maxTimeoutInSeconds)
				{
					hasTimedOut = true;
					break;
				}
				timer += Time.deltaTime;
				yield return null;
			}
			if (hasTimedOut || !string.IsNullOrEmpty(request.error))
			{
				errorHandler(request.error);
			}
			else
			{
				responseHandler(request.text);
			}
			request.Dispose();
		}
	}
}
