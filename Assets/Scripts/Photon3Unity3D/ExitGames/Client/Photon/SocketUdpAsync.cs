using System;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Threading;

namespace ExitGames.Client.Photon
{
	public class SocketUdpAsync : IPhotonSocket, IDisposable
	{
		private Socket sock;

		private readonly object syncer = new object();

		public SocketUdpAsync(PeerBase npeer)
			: base(npeer)
		{
			if (ReportDebugOfLevel(DebugLevel.INFO))
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "SocketUdpAsync: UDP, Unity3d.");
			}
			PollReceive = false;
		}

		public override bool Connect()
		{
			lock (syncer)
			{
				if (!base.Connect())
				{
					return false;
				}
			}
			base.State = PhotonSocketState.Connecting;
			Thread thread = new Thread(DnsAndConnect);
			thread.Name = "photon dns thread";
			thread.IsBackground = true;
			thread.Start();
			return true;
		}

		internal void DnsAndConnect()
		{
			IPAddress iPAddress = null;
			try
			{
				iPAddress = IPhotonSocket.GetIpAddress(base.ServerAddress);
				if (iPAddress == null)
				{
					throw new ArgumentException("Invalid IPAddress. Address: " + base.ServerAddress);
				}
				lock (syncer)
				{
					if (base.State == PhotonSocketState.Disconnecting || base.State == PhotonSocketState.Disconnected)
					{
						return;
					}
					if (iPAddress.AddressFamily != AddressFamily.InterNetwork && iPAddress.AddressFamily != AddressFamily.InterNetworkV6)
					{
						throw new ArgumentException(string.Concat("AddressFamily '", iPAddress.AddressFamily, "' not supported. Address: ", base.ServerAddress));
					}
					sock = new Socket(iPAddress.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
					sock.Connect(iPAddress, base.ServerPort);
					base.AddressResolvedAsIpv6 = IsIpv6SimpleCheck(iPAddress);
					base.State = PhotonSocketState.Connected;
					peerBase.OnConnect();
				}
			}
			catch (SecurityException ex)
			{
				if (ReportDebugOfLevel(DebugLevel.ERROR))
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Connect() to '" + base.ServerAddress + "' (" + ((iPAddress == null) ? "" : iPAddress.AddressFamily.ToString()) + ") failed: " + ex.ToString());
				}
				HandleException(StatusCode.SecurityExceptionOnConnect);
				return;
			}
			catch (Exception ex2)
			{
				if (ReportDebugOfLevel(DebugLevel.ERROR))
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Connect() to '" + base.ServerAddress + "' (" + ((iPAddress == null) ? "" : iPAddress.AddressFamily.ToString()) + ") failed: " + ex2.ToString());
				}
				HandleException(StatusCode.ExceptionOnConnect);
				return;
			}
			StartReceive();
		}

		public override PhotonSocketError Send(byte[] data, int length)
		{
			lock (syncer)
			{
				if (sock == null || !sock.Connected)
				{
					return PhotonSocketError.Skipped;
				}
				try
				{
					sock.Send(data, 0, length, SocketFlags.None);
				}
				catch (Exception ex)
				{
					if (ReportDebugOfLevel(DebugLevel.ERROR))
					{
						EnqueueDebugReturn(DebugLevel.ERROR, "Cannot send to: " + base.ServerAddress + ". " + ex.Message);
					}
					return PhotonSocketError.Exception;
				}
			}
			return PhotonSocketError.Success;
		}

		public override PhotonSocketError Receive(out byte[] data)
		{
			data = null;
			return PhotonSocketError.NoData;
		}

		public void StartReceive()
		{
			byte[] array = new byte[base.MTU];
			try
			{
				sock.BeginReceive(array, 0, array.Length, SocketFlags.None, OnReceive, array);
			}
			catch (Exception ex)
			{
				if (base.State != PhotonSocketState.Disconnecting && base.State != PhotonSocketState.Disconnected)
				{
					if (ReportDebugOfLevel(DebugLevel.ERROR))
					{
						EnqueueDebugReturn(DebugLevel.ERROR, string.Concat("Receive issue. State: ", base.State, ". Server: '", base.ServerAddress, "' Exception: ", ex));
					}
					HandleException(StatusCode.ExceptionOnReceive);
				}
			}
		}

		private void OnReceive(IAsyncResult ar)
		{
			if (base.State == PhotonSocketState.Disconnecting || base.State == PhotonSocketState.Disconnected)
			{
				return;
			}
			int length = 0;
			try
			{
				length = sock.EndReceive(ar);
			}
			catch (SocketException ex)
			{
				if (base.State != PhotonSocketState.Disconnecting && base.State != PhotonSocketState.Disconnected)
				{
					if (ReportDebugOfLevel(DebugLevel.ERROR))
					{
						EnqueueDebugReturn(DebugLevel.ERROR, string.Concat("SocketException in EndReceive. State: ", base.State, ". Server: '", base.ServerAddress, "' ErrorCode: ", ex.ErrorCode, " SocketErrorCode: ", ex.SocketErrorCode, " Message: ", ex.Message, " ", ex));
					}
					HandleException(StatusCode.ExceptionOnReceive);
				}
			}
			catch (Exception ex2)
			{
				if (base.State != PhotonSocketState.Disconnecting && base.State != PhotonSocketState.Disconnected)
				{
					if (ReportDebugOfLevel(DebugLevel.ERROR))
					{
						EnqueueDebugReturn(DebugLevel.ERROR, string.Concat("Exception in EndReceive. State: ", base.State, ". Server: '", base.ServerAddress, "' Exception: ", ex2));
					}
					HandleException(StatusCode.ExceptionOnReceive);
				}
			}
			if (base.State == PhotonSocketState.Disconnecting || base.State == PhotonSocketState.Disconnected)
			{
				return;
			}
			byte[] array = (byte[])ar.AsyncState;
			HandleReceivedDatagram(array, length, true);
			try
			{
				sock.BeginReceive(array, 0, array.Length, SocketFlags.None, OnReceive, array);
			}
			catch (SocketException ex3)
			{
				if (base.State != PhotonSocketState.Disconnecting && base.State != PhotonSocketState.Disconnected)
				{
					if (ReportDebugOfLevel(DebugLevel.ERROR))
					{
						EnqueueDebugReturn(DebugLevel.ERROR, string.Concat("SocketException in BeginReceive. State: ", base.State, ". Server: '", base.ServerAddress, "' ErrorCode: ", ex3.ErrorCode, " SocketErrorCode: ", ex3.SocketErrorCode, " Message: ", ex3.Message, " ", ex3));
					}
					HandleException(StatusCode.ExceptionOnReceive);
				}
			}
			catch (Exception ex4)
			{
				if (base.State != PhotonSocketState.Disconnecting && base.State != PhotonSocketState.Disconnected)
				{
					if (ReportDebugOfLevel(DebugLevel.ERROR))
					{
						EnqueueDebugReturn(DebugLevel.ERROR, string.Concat("Exception in BeginReceive. State: ", base.State, ". Server: '", base.ServerAddress, "' Exception: ", ex4));
					}
					HandleException(StatusCode.ExceptionOnReceive);
				}
			}
		}

		public override bool Disconnect()
		{
			if (ReportDebugOfLevel(DebugLevel.INFO))
			{
				EnqueueDebugReturn(DebugLevel.INFO, "SocketUdpAsync.Disconnect()");
			}
			base.State = PhotonSocketState.Disconnecting;
			lock (syncer)
			{
				if (sock != null)
				{
					try
					{
						sock.Close();
					}
					catch (Exception ex)
					{
						EnqueueDebugReturn(DebugLevel.INFO, "Exception in Disconnect(): " + ex);
					}
					sock = null;
				}
			}
			base.State = PhotonSocketState.Disconnected;
			return true;
		}

		public void Dispose()
		{
			base.State = PhotonSocketState.Disconnecting;
			if (sock != null)
			{
				try
				{
					if (sock.Connected)
					{
						sock.Close();
					}
				}
				catch (Exception ex)
				{
					EnqueueDebugReturn(DebugLevel.INFO, "Exception in Dispose(): " + ex);
				}
			}
			sock = null;
			base.State = PhotonSocketState.Disconnected;
		}
	}
}
