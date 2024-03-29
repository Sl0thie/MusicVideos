﻿namespace MusicVideosRemote.Controls
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
        /// <summary>
        /// Bindable Value property.
        /// </summary>
        public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(int), typeof(SliderView), null, BindingMode.TwoWay);

        /// <summary>
        /// Gets or sets the title of the control.
        /// </summary>
        public string ControlTitle
        {
            get
            {
                return controlTitle;
            }

            set
            {
                controlTitle = value;
                DisplayTitle.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the value property.
        /// </summary>
        public int Value
        {
            get
            {
                return (int)GetValue(ValueProperty);
            }

            set
            {
                SetValue(ValueProperty, value);
                Adjuster.Value = Value;
                DisplayValue.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the minimum for the slider.
        /// </summary>
        public int Minimum
        {
            get
            {
                return minimum;
            }

            set
            {
                if (Value < value)
                {
                    Value = value;
                }

                minimum = value;
                Adjuster.Minimum = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum for the slider.
        /// </summary>
        public int Maximum
        {
            get
            {
                return maximum;
            }

            set
            {
                if (Value > value)
                {
                    Value = value;
                }

                maximum = value;
                Adjuster.Maximum = value;
            }
        }

        private string controlTitle;
        private int minimum = 0;
        private int maximum = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderView"/> class.
        /// </summary>
        public SliderView()
        {
            InitializeComponent();
        }

        private void Decrease_Clicked(object sender, EventArgs e)
        {
            Value--;
            if (Value < minimum)
            {
                Value = minimum;
            }

            Adjuster.Value = Value;
        }

        private void Increase_Clicked(object sender, EventArgs e)
        {
            Value++;
            if (Value > maximum)
            {
                Value = maximum;
            }

            Adjuster.Value = Value;
        }
    }
}