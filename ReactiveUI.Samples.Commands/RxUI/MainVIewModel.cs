using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ReactiveUI.Samples.Commands.RxUI
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            DisplayCommand = ReactiveCommand.Create(this.WhenAny(x => x.Name, x => !string.IsNullOrEmpty(x.Value)));
            DisplayCommand.Subscribe(_ => MessageBox.Show("You clicked on DisplayCommand: Name is " + Name));

            Action<long> incrementProgress = (_) => Progress = (Progress + 5) % 100;
            var smallRandomNumber = new Random(DateTime.Now.Millisecond).Next(10);
            var countUp = Observable.Interval(TimeSpan.FromSeconds(0.1)).Take(smallRandomNumber);
            StartAsyncCommand = ReactiveCommand.CreateAsyncTask(
                async _ =>
                {
                    await countUp.ForEachAsync(incrementProgress);
                    return AsyncVoid.Default; 
                });

            // XXX: What's the difference between CreateAyncTask and Create(), followed by Subscribe?
            // This documention isn't clicking - http://reactiveui.readthedocs.org/en/stable/basics/reactive-command-async/
            var StartAsyncCommand2 = ReactiveCommand.Create();
            StartAsyncCommand2.Subscribe(_=>countUp.ForEachAsync(incrementProgress));
        }
        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { this.RaiseAndSetIfChanged(ref _Name, value); }
        }

        public ReactiveCommand<object> DisplayCommand { get; protected set; }

        private int _Progress;
        public int Progress
        {
            get { return _Progress; }
            set { this.RaiseAndSetIfChanged(ref _Progress, value); }
        }
        public ReactiveCommand<AsyncVoid> StartAsyncCommand { get; protected set; }
    }
}