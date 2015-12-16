using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Reactive;

namespace ReactiveUI.Samples.Basics.ViewModels
{
    public class MainViewModel : ReactiveObject
    {

        public MainViewModel()
        {
            RxApp.MainThreadScheduler = new DispatcherScheduler(Application.Current.Dispatcher);
            var slowTicks = Observable.Interval(TimeSpan.FromSeconds(5));
            var fastTicks = Observable.Interval(TimeSpan.FromSeconds(0.5));
            Func<bool> isProgressMultipleOf10 = ()=> Progress%10 == 0;
            Action<long,int> IncrementProgress = (_,__) => Progress = (Progress + 1)%100;

            // Kind of weird, but experssive, I think this leaks, need to figure out how to avoid that.
            slowTicks.Where(_=>isProgressMultipleOf10()).ForEachAsync(IncrementProgress);
            fastTicks.Where(_=>!isProgressMultipleOf10()).ForEachAsync(IncrementProgress);

            // For R/W properties, write it on someone elses subscription.
            this.ObservableForProperty(vm => vm.Progress)
                .Where((x,index)=> (index % 4) == 0)
                .Subscribe(c =>
                {
                    SlowProgress = Progress;
                });


            // For (read only) subscription only properties - use ToProperty.
            this.WhenAnyValue(vm => vm.Progress)
                .Select(progress=>(progress*2)%100)
                .Delay(TimeSpan.FromSeconds(0.2))
                .ToProperty(this, vm => vm.SlowProgress2, out slowProgress2,initialValue:50);

            // You can do arbitray subscriptions on the observable.
            this.WhenAny(
                    vm => vm.Progress, 
                    vm => vm.SlowProgress2, 
                    vm => vm.SlowProgress,
                    (progress,sp2,sp) =>  $"P:{progress.Value} SP:{sp.Value};SP2:{sp2.Value}"
                ).Subscribe(s=>Debug.WriteLine(s));


            // NOTES: WhenAnyValue vs ObservableForProperty  vs WhenAny vs WhenAnyObservable
            // http://reactiveui.readthedocs.org/en/stable/basics/whenany/
                // OFP -> single property  -can't pass initial value
                // WAV -> single property - can pass initial value
                // WA -> multiple property - can pass initial value
                // TBD: WAO

            Person = new PersonViewModel();
            Calculator = new CalculatorViewModel();
        }

        private int _Progress;
        public int Progress
        {
            get { return _Progress; }
            set { this.RaiseAndSetIfChanged(ref _Progress, value); }
        }
        private int _SlowProgress;

        public int SlowProgress
        {
            get { return _SlowProgress; }
            set { this.RaiseAndSetIfChanged(ref _SlowProgress, value); }
        }

        readonly ObservableAsPropertyHelper<int> slowProgress2;
        public int SlowProgress2 => slowProgress2.Value;

        private PersonViewModel _Person;

        public PersonViewModel Person
        {
            get { return _Person; }
            set { this.RaiseAndSetIfChanged(ref _Person, value); }
        }

        private CalculatorViewModel _Calculator;

        public CalculatorViewModel Calculator
        {
            get { return _Calculator; }
            set { this.RaiseAndSetIfChanged(ref _Calculator, value); }
        }
    }
}