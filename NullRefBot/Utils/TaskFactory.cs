using System;
using System.Threading.Tasks;

namespace NullRefBot.Utils {
	public class TaskUtils {
		public static Task Run ( Action action ) {
			return Task.Factory.StartNew( action ).ContinueWith( c => {
				var e = c.Exception;
				if( e != null ) {
					Console.WriteLine( e );
					throw e;
				}
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously );
		}
		public static Task<T> Run<T> ( Func<T> func ) {
			return Task.Factory.StartNew( func ).ContinueWith( c => {
				var e = c.Exception;
				if( e != null ) {
					Console.WriteLine( e );
					throw e;
				}
				return default(T);
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously );
		}
		public static Task<T> Run<T> ( Func<Task<T>> func ) {
			return Task.Factory.StartNew( func ).ContinueWith( c => {
				var e = c.Exception;
				if( e != null ) {
					Console.WriteLine( e );
					throw e;
				}
				return default(T);
			}, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously );
		}
	}
}
