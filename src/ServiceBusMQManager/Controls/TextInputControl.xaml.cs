#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    TextInputControl.xaml.cs
  Created: 2012-11-22

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System.Windows.Media;
#region File Information
/********************************************************************
  Project: ServiceBusMQManager
  File:    TextInputControl.xaml.cs
  Created: 2012-11-22

  Author(s):
    Daniel Halan

 (C) Copyright 2012 Ingenious Technology with Quality Sweden AB
     all rights reserved

********************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ServiceBusMQ;

namespace ServiceBusMQManager.Controls {
  /// <summary>
  /// Interaction logic for TextInputControl.xaml
  /// </summary>
  public partial class TextInputControl : UserControl, IInputControl {

    readonly SolidColorBrush BORDER_SELECTED = new SolidColorBrush(Color.FromRgb(78, 166, 234));
    readonly SolidColorBrush BORDER_NORMAL = new SolidColorBrush(Colors.Black);
    readonly SolidColorBrush BORDER_LISTITEM = new SolidColorBrush(Color.FromRgb(201,201,201));



    Type _dataType;
    private bool _isListItem;
    bool _isNullable;


    object _value;

    bool _updating;


    public TextInputControl() {
      InitializeComponent();
      
      tb.Height = 30;
    }


    public TextInputControl(object value, Type dataType, bool isNullable) {
      InitializeComponent();

      tb.Height = 30;

      Init(value, dataType, isNullable);
    }


    public void Init(object value, Type dataType, bool isNullable) {

      _isNullable = isNullable;
      _dataType = dataType;
      _value = value;


      SetTextBoxValue(value);

      BindDataType();

      UpdateBorder();
    }


    private void BindDataType() {

      tb.Tag = "TEXT";


      if( _dataType.IsGuid() ) {

        tb.Margin = new Thickness(0, 0, 80, 0);

        btn.Content = "GENERATE";
        btn.Click += btnGuid_Click;
        btn.Visibility = System.Windows.Visibility.Visible;


        tb.Tag = "GUID";
      }

    }

    void btnGuid_Click(object sender, RoutedEventArgs e) {
      tb.Text = Guid.NewGuid().ToString().ToUpper();
    }


    void SetTextBoxValue(object value) {
      
      if( value != null ) {

        if( value is string )
          tb.Text = (string)value;

        else if( value is Guid )
          tb.Text = value.ToString().ToUpper();

        else tb.Text = value.ToString();
      
      } else tb.Text = string.Empty; 

    }

    public void UpdateValue(object value) {
      _updating = true;
      try {
        _value = value;

        SetTextBoxValue(value);

      } finally {
        _updating = false;
      }
    }



    bool UpdateValueFromControl() {

      try { 
        _value = Tools.Convert(tb.Text, _dataType);
      
      } catch(NotSupportedException e) {
        throw e;

      } catch {
        return false;
      }


      return true;
    }


    public object RetrieveValue() {

      UpdateValueFromControl();

      return _value;
    }

    public bool IsListItem {
      get {
        return _isListItem;
      }
      set {
        _isListItem = value;
        ListItemStateChanged();
      }
    }

    private void ListItemStateChanged() {

      if( _isListItem ) {
        tb.Foreground = Brushes.White;
        tb.Background = Brushes.Gray;

        if( btn != null ) {
          btn.Visibility = System.Windows.Visibility.Hidden;
          tb.Margin = new Thickness(0, 0, 0, 0);
        }
      }

      UpdateBorder();
    }


    public event EventHandler<EventArgs> ValueChanged;
    void OnValueChanged() {
      if( ValueChanged != null )
        ValueChanged(this, EventArgs.Empty);
    }


    bool _isValidValue = true;

    private void tb_TextChanged(object sender, TextChangedEventArgs e) {
      if( !_updating ) {

        try {
          _isValidValue = UpdateValueFromControl();
           UpdateBorder();

        } catch { }

        OnValueChanged();
      }
    }


    private void UpdateBorder() {
      if( !_isValidValue ) {
        tb.BorderBrush = Brushes.Red;

      } else if( tb.IsFocused ) {
        tb.BorderBrush = BORDER_SELECTED;
        tb.BorderThickness = new Thickness(1, 2, 2, 2);
      } else {

        if( !_isListItem ) {
          tb.BorderThickness = new Thickness(1);
          tb.BorderBrush = BORDER_NORMAL;
        
        } else {
          tb.BorderThickness = new Thickness(1,1,0,1);
          tb.BorderBrush = BORDER_LISTITEM;
        }
      }
    }

    private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e) {

      
      if( _dataType.IsInteger() ) {
        if( !e.Text.IsInt32() )
          e.Handled = true;


      } else if( _dataType.IsDecimal() )  {
        if( !e.Text.IsDecimal() )
          e.Handled = true;

      } else if( _dataType.IsAnyFloatType() ) {
        if( !e.Text.IsDouble() )
          e.Handled = true;
      }

    }

    private void tb_LostFocus(object sender, RoutedEventArgs e) {
      UpdateBorder();
    }

    private void tb_GotFocus(object sender, RoutedEventArgs e) {
      UpdateBorder();
      
      if( SelectAllTextOnFocus )
        tb.SelectAll();
    }

    public bool SelectAllTextOnFocus { get; set; }

    internal void SelectAll() {
      tb.SelectAll();
    }

    internal void FocusTextBox() {
      tb.Focus();
    }
  }
}