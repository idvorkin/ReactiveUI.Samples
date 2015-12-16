using System;
using Splat;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ReactiveUI.Samples.Basics.ViewModels
{
    public class CalculatorViewModel : ReactiveValidatedObject
    {
        private MemoizingMRUCache<int, int> _calculationMemoizer;

        public CalculatorViewModel()
        {
            _calculationMemoizer = new MemoizingMRUCache<int, int>((x, ctx) =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                return x*10;
            }, maxSize:5);


            CalculateCommand = ReactiveCommand.CreateAsyncTask(o => {
                return Task.Factory.StartNew(() =>
                {
                    int cachedAnswer;
                    OutputLine = "Executing Calculate Command";
                    bool cached = _calculationMemoizer.TryGet(    InputNumber, out cachedAnswer);
                    if (cached)
                    {
                        // set it to -1 so you can see it's cached.
                        OutputLine = "Value cached - reading it";
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        Result = cachedAnswer;
                        OutputLine = "Done";
                    }
                    else
                    {
                        // set it to -2 so you can see it's being computed.
                        OutputLine = "Value not cached, calling memoizer";
                        cachedAnswer = _calculationMemoizer.Get(InputNumber);
                        for (int i = 0; i <= cachedAnswer; i++)
                        {
                            // Count up from 0 to the answer slowly.
                            Result = i;
                            Thread.Sleep(100);
                        }
                        OutputLine = "Done";
                    }
                });
            });

        }

        private int _inputNumber;
        [Required]
        public int InputNumber
        {
            get { return _inputNumber; }
            set { this.RaiseAndSetIfChanged(ref _inputNumber, value); }
        }

        public ICommand CalculateCommand { get; set; }

        private int _Result;
        public int Result
        {
            get { return _Result; }
            set { this.RaiseAndSetIfChanged(ref _Result, value); }
        }
        private string _OutputLine;
        public string OutputLine
        {
            get { return _OutputLine; }
            set { this.RaiseAndSetIfChanged(ref _OutputLine, value); }
        }
    }
}