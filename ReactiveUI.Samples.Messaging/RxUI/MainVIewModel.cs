using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace ReactiveUI.Samples.Messaging.RxUI
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            Publisher = new PublisherViewModel();
            Subscriber = new SubscriberViewModel();
        }
        public PublisherViewModel Publisher { get; set; }
        public SubscriberViewModel Subscriber { get; set; }
         
    }

    public class PublisherViewModel : ReactiveObject
    {
        public PublisherViewModel()
        {
            PublishCommand = ReactiveCommand.Create();
            MessageBus.Current.RegisterMessageSource(PublishCommand);
        }

        public IReactiveCommand<object> PublishCommand { get; protected set; }
    }

    public class SubscriberViewModel : ReactiveObject
    {
        readonly ObservableAsPropertyHelper<int> value;
        public int Value => value.Value;
        public SubscriberViewModel()
        {
            MessageBus.Current.Listen<object>()
            .Select(_=>this.Value+1)
            .ToProperty(this, vm => vm.Value, out value);
        }
    }


}