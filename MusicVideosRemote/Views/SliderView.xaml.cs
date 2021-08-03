namespace MusicVideosRemote
{
    using System;
    using System.Diagnostics;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    /// <summary>
    /// SliderView class.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SliderView : ContentView
    {
        private string controlTitle;

        /// <summary>
        /// Gets or sets the title of the control.
        /// </summary>
        public string ControlTitle
        {
            get
            {
                Debug.WriteLine("SliderView.ControlTitle.Get");

                return controlTitle;
            }

            set
            {
                Debug.WriteLine("SliderView.ControlTitle.Set");

                controlTitle = value;
                DisplayTitle.Text = value;
            }
        }

        /// <summary>
        /// Bindable Value property.
        /// </summary>
        public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(int), typeof(SliderView), null, BindingMode.TwoWay);

        /// <summary>
        /// Gets or sets the value property.
        /// </summary>
        public int Value
        {
            get
            {
                Debug.WriteLine("SliderView.Value.Get " + controlTitle + " " + GetValue(ValueProperty));

                return (int)GetValue(ValueProperty);
            }

            set
            {
                Debug.WriteLine("SliderView.Value.Set " + controlTitle + " " + value);

                SetValue(ValueProperty, value);
                Adjuster.Value = Value;
                DisplayValue.Text = value.ToString();
            }
        }

        private int minimum = 0;

        /// <summary>
        /// Gets or sets the minimum for the slider.
        /// </summary>
        public int Minimum
        {
            get
            {
                Debug.WriteLine("SliderView.Minimum.Get " + controlTitle);

                return minimum;
            }

            set
            {
                Debug.WriteLine("SliderView.Minimum.Set " + controlTitle);

                if (Value < value)
                {
                    Value = value;
                }

                minimum = value;
                Adjuster.Minimum = value;
            }
        }

        private int maximum = 100;

        /// <summary>
        /// Gets or sets the maximum for the slider.
        /// </summary>
        public int Maximum
        {
            get
            {
                Debug.WriteLine("SliderView.Maximum.Get " + controlTitle);

                return maximum;
            }

            set
            {
                Debug.WriteLine("SliderView.Maximum.Set " + controlTitle);

                if (Value > value)
                {
                    Value = value;
                }

                maximum = value;
                Adjuster.Maximum = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderView"/> class.
        /// </summary>
        public SliderView()
        {
            Debug.WriteLine("SliderView.SliderView");

            InitializeComponent();
        }

        private void Decrease_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("SliderView.Decrease_Clicked");

            Value--;
            if (Value < minimum)
            {
                Value = minimum;
            }

            Adjuster.Value = Value;
        }

        private void Increase_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("SliderView.Increase_Clicked");

            Value++;
            if (Value > maximum)
            {
                Value = maximum;
            }

            Adjuster.Value = Value;
        }
    }
}