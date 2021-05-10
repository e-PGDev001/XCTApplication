using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Xamarin.Forms;

namespace XCTApplication.Controls
{
    public class CrossLabel : Label
    {
        public event EventHandler<EventArgs> Clicked;

        public CrossLabel()
        {
            this.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = ReactiveCommand.CreateFromTask(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                    this.Command?.Execute(this.CommandParameter);

                    //raise event
                    this.Clicked?.Invoke(this, new EventArgs());
                })
            });
        }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(CrossLabel));

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CrossLabel));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
    }
}