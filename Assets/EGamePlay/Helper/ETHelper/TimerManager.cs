using System;
using System.Collections.Generic;
using EGamePlay;

namespace ET
{
	public interface ITimer
	{
		void Run(bool isTimeout);
	}
	public class OnceWaitTimer: Entity, ITimer
	{
		private ETTaskCompletionSource<bool> Callback { get; set; }
        public override void Awake(object initData)
        {
			Callback = initData as ETTaskCompletionSource<bool>;
		}
        public void Run(bool isTimeout)
		{
			var tcs = Callback;
			GetParent<TimerManager>().Remove(Id);
			tcs.SetResult(isTimeout);
		}
	}

	public class OnceTimer: Entity, ITimer
	{
		private Action<bool> Callback { get; set; }
		public override void Awake(object initData)
		{
			Callback = initData as Action<bool>;
		}
		public void Run(bool isTimeout)
		{
			try
			{
				Callback.Invoke(isTimeout);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}

	public class RepeatedTimerAwakeData
	{
		public long RepeatedTime;
		public Action<bool> Callback;
    }

	public class RepeatedTimer: Entity, ITimer
	{
		public override void Awake(object initData)
		{
			var awakeData = initData as RepeatedTimerAwakeData;
			StartTime = TimeHelper.ClientNow();
			if (awakeData != null)
			{
				RepeatedTime = awakeData.RepeatedTime;
				Callback = awakeData.Callback;
			}
			Count = 1;
		}
		private long StartTime { get; set; }
		private long RepeatedTime { get; set; }
		// 下次一是第几次触发
		private int Count { get; set; }
		private Action<bool> Callback { get; set; }
		public void Run(bool isTimeout)
		{
			++Count;
			var timerComponent = GetParent<TimerManager>();
			var tillTime = StartTime + RepeatedTime * Count;
			timerComponent.AddToTimeId(tillTime, Id);
			try
			{
				Callback?.Invoke(isTimeout);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		public override void OnDestroy()
		{
			if (IsDisposed) return;
			var id = Id;
			if (id == 0)
			{
				Log.Error($"RepeatedTimer可能多次释放了");
				return;
			}
			StartTime = 0;
			RepeatedTime = 0;
			Callback = null;
			Count = 0;
		}
	}
	public class TimerManager : Entity
	{
		public static TimerManager Instance { get; set; }
		private readonly Dictionary<long, ITimer> timers = new ();
		/// <summary>
		/// key: time, value: timer id
		/// </summary>
		private readonly MultiMap<long, long> timeId = new ();
		private readonly Queue<long> timeOutTime = new ();
		private readonly Queue<long> timeOutTimerIds = new ();
		// 记录最小时间，不用每次都去MultiMap取第一个值
		private long minTime;
        public override void Awake()
        {
			Instance = this;
        }
        public new void Update()
		{
			if (timeId.Count == 0) return;
			var timeNow = TimeHelper.ClientNow();
			if (timeNow < minTime) return;
			foreach (var kv in timeId.GetDictionary())
			{
				var k = kv.Key;
				if (k > timeNow)
				{
					minTime = k;
					break;
				}
				timeOutTime.Enqueue(k);
			}

			while(timeOutTime.Count > 0)
			{
				var time = timeOutTime.Dequeue();
				foreach(var timerId in timeId[time])
				{
					timeOutTimerIds.Enqueue(timerId);	
				}
				timeId.Remove(time);
			}
			while(timeOutTimerIds.Count > 0)
			{
				var timerId = timeOutTimerIds.Dequeue();
				if (!timers.TryGetValue(timerId, out var timer)) continue;
				timer.Run(true);
			}
		}

		public async ETTask<bool> WaitTillAsync(long tillTime, ETCancellationToken cancellationToken)
		{
			if (TimeHelper.ClientNow() > tillTime)
			{
				return true;
			}
			ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
			OnceWaitTimer timer = this.AddChild<OnceWaitTimer>(tcs);
			this.timers[timer.Id] = timer;
			AddToTimeId(tillTime, timer.Id);
			
			long instanceId = timer.InstanceId;
			cancellationToken.Register(() =>
			{
				if (instanceId != timer.InstanceId)
				{
					return;
				}
				
				timer.Run(false);
				
				this.Remove(timer.Id);
			});
			return await tcs.Task;
		}

		public async ETTask<bool> WaitTillAsync(long tillTime)
		{
			if (TimeHelper.ClientNow() > tillTime)
			{
				return true;
			}
			ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
			OnceWaitTimer timer = this.AddChild<OnceWaitTimer>(tcs);
			this.timers[timer.Id] = timer;
			AddToTimeId(tillTime, timer.Id);
			return await tcs.Task;
		}

		public async ETTask<bool> WaitAsync(long time, ETCancellationToken cancellationToken)
		{
			long tillTime = TimeHelper.ClientNow() + time;

            if (TimeHelper.ClientNow() > tillTime)
            {
                return true;
            }

            ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
			OnceWaitTimer timer = this.AddChild<OnceWaitTimer>(tcs);
			this.timers[timer.Id] = timer;
			AddToTimeId(tillTime, timer.Id);
			long instanceId = timer.InstanceId;
			cancellationToken.Register(() =>
			{
				if (instanceId != timer.InstanceId)
				{
					return;
				}
				
				timer.Run(false);
				
				this.Remove(timer.Id);
			});
			return await tcs.Task;
		}

		public async ETTask<bool> WaitAsync(long time)
		{
			long tillTime = TimeHelper.ClientNow() + time;
			ETTaskCompletionSource<bool> tcs = new ETTaskCompletionSource<bool>();
			OnceWaitTimer timer = this.AddChild<OnceWaitTimer>(tcs);
			this.timers[timer.Id] = timer;
			AddToTimeId(tillTime, timer.Id);
			return await tcs.Task;
		}

		/// <summary>
		/// 创建一个RepeatedTimer
		/// </summary>
		/// <param name="time"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		public long NewRepeatedTimer(long time, Action<bool> action)
		{
			if (time < 30)
			{
				throw new Exception($"repeated time < 30");
			}
			long tillTime = TimeHelper.ClientNow() + time;
			RepeatedTimer timer = this.AddChild<RepeatedTimer>(new RepeatedTimerAwakeData() { Callback = action, RepeatedTime = time });
			this.timers[timer.Id] = timer;
			AddToTimeId(tillTime, timer.Id);
			return timer.Id;
		}
		
		public RepeatedTimer GetRepeatedTimer(long id)
		{
			if (!this.timers.TryGetValue(id, out ITimer timer))
			{
				return null;
			}
			return timer as RepeatedTimer;
		}
		
		public void Remove(long id)
		{
			if (id == 0)
			{
				return;
			}
			ITimer timer;
			if (!this.timers.TryGetValue(id, out timer))
			{
				return;
			}
			this.timers.Remove(id);
			
			(timer as IDisposable)?.Dispose();
		}
		
		public long NewOnceTimer(long tillTime, Action action)
		{
			OnceTimer timer = this.AddChild<OnceTimer>(action);
			this.timers[timer.Id] = timer;
			AddToTimeId(tillTime, timer.Id);
			return timer.Id;
		}
		
		public OnceTimer GetOnceTimer(long id)
		{
			if (!this.timers.TryGetValue(id, out ITimer timer))
			{
				return null;
			}
			return timer as OnceTimer;
		}

		public void AddToTimeId(long tillTime, long id)
		{
			this.timeId.Add(tillTime, id);
			if (tillTime < this.minTime)
			{
				this.minTime = tillTime;
			}
		}
	}
}