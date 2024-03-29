﻿using Android.Animation;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;

using Mimansikon.Util;

using System;

using static Android.Views.ViewGroup.LayoutParams;

namespace Mimansikon.ViewUtil {
	public class Dialog : IDisposable {
		public readonly LinearLayout View;
		public readonly LinearLayout ContentView;
		public readonly LinearLayout Buttons;
		public bool Closeable { get; set; } = true;

		protected readonly Activity Context;
		public Dialog(Activity context) {
			Context = context;

			View = new LinearLayout(context);
			View.SetBackgroundColor(Color.White);
			View.Orientation = Orientation.Vertical;
			View.SetGravity(GravityFlags.Center);

			TitleView = new TextView(context) {
				TextSize = TypedValue.ApplyDimension(ComplexUnitType.Sp, 6, Screen.DisplayMetrics),
				Gravity = GravityFlags.Left | GravityFlags.Center,
				Visibility = ViewStates.Gone
			};
			int pd = 5.Dip();
			TitleView.SetPadding(pd, pd, pd, pd);
			View.AddView(TitleView);

			ContentView = new LinearLayout(context) {
				LayoutParameters = new LinearLayout.LayoutParams(MatchParent, WrapContent, 1f)
			};
			View.AddView(ContentView);

			Buttons = new LinearLayout(context) {
				Orientation = Orientation.Horizontal
			};
			Buttons.SetGravity(GravityFlags.Right);
			View.AddView(Buttons);

			OnCreate();
		}




		public delegate bool OnButtonClick(Button button);
		public Button AddButton(int res, OnButtonClick onclick) {
			return AddButton(Context.GetString(res), onclick);
		}

		public Button AddButton(String text, OnButtonClick onclick) {
			Button button = new Button(Context) {
				LayoutParameters = new LinearLayout.LayoutParams(WrapContent, WrapContent, 1f),
				Text = text,
				Color = new Color(Context.GetColor(Resource.Color.colorPrimaryDark)),
			};
			button.SetTextColor(Color.White);

			button.Click += delegate {
				if(onclick(button)) {
					Close();
				}
			};
			Buttons.AddView(button);
			return button;
		}


		int Width;
		int Height;
		public void SetSize(int width, int height) {
			Width = width;
			Height = height;
		}

		protected void SetContentView(View view) {
			ContentView.RemoveAllViews();
			ContentView.AddView(view);
		}
		protected virtual void OnCreate() {
		}

		protected String Title {
			get { return TitleView.Text; }
			set {
				TitleView.Text = value;
				TitleView.Visibility = ViewStates.Visible;
			}
		}
		protected Color TitleColor {
			get {
				return ((ColorDrawable) TitleView.Background).Color;
			}
			set {
				TitleView.SetBackgroundColor(value);
			}
		}
		protected Color TitleTextColor {
			get {
				return new Color(TitleView.CurrentTextColor);
			}
			set {
				TitleView.SetTextColor(value);
			}
		}
		private TextView TitleView { get; set; }



		#region Showing & Closing
		public Rect ShowCoords;
		public void Show() {
			if(ShowCoords == null) {
				int x = Screen.Width >> 1;
				int y = Screen.Height >> 1;
				ShowCoords = new Rect(x, y, x, y);
			}

			View.LayoutParameters = new RelativeLayout.LayoutParams(ShowCoords.Width(), ShowCoords.Height()) {
				LeftMargin = ShowCoords.Left,
				TopMargin = ShowCoords.Top
			};
			Context.AddDialog(this);
			AnimateOpening();
		}

		public void Close() {
			AnimateClosing();
		}

		private ValueAnimator OpenAnimator;
		public void AnimateOpening() {
			if(CloseAnimator != null && CloseAnimator.IsRunning) return;
			if(OpenAnimator != null && OpenAnimator.IsRunning) return;

			OpenAnimator = ValueAnimator.OfFloat(0, 1);
			OpenAnimator.Update += delegate {
				float anim = (float) Math.Sin(OpenAnimator.AnimatedFraction * Math.PI / 2);
				int x = ShowCoords.Left + (int) Math.Round((((Screen.Width - Width) >> 1) - ShowCoords.Left) * anim);
				int y = ShowCoords.Top + (int) Math.Round((((Screen.Height - Height) >> 1) - ShowCoords.Top) * anim);

				int width = (int) Math.Round(ShowCoords.Width() + (Width - ShowCoords.Width()) * anim);
				int height = (int) Math.Round(ShowCoords.Height() + (Height - ShowCoords.Height()) * anim);

				View.Alpha = anim;
				View.LayoutParameters = new RelativeLayout.LayoutParams(width, height) {
					LeftMargin = x,
					TopMargin = y
				};
			};
			OpenAnimator.AnimationEnd += delegate {
				OpenAnimator.Dispose();
				OpenAnimator = null;
			};
			OpenAnimator.SetDuration(200);
			OpenAnimator.Start();
		}

		private ValueAnimator CloseAnimator;
		public void AnimateClosing() {
			if(OpenAnimator != null && OpenAnimator.IsRunning) return;
			if(CloseAnimator != null && CloseAnimator.IsRunning) return;

			CloseAnimator = ValueAnimator.OfFloat(0, 1);
			CloseAnimator.Update += delegate {
				float anim = (float) Math.Sin((1 - CloseAnimator.AnimatedFraction) * Math.PI / 2);
				int x = ShowCoords.Left + (int) Math.Round((((Screen.Width - Width) >> 1) - ShowCoords.Left) * anim);
				int y = ShowCoords.Top + (int) Math.Round((((Screen.Height - Height) >> 1) - ShowCoords.Top) * anim);

				int width = (int) Math.Round(ShowCoords.Width() + (Width - ShowCoords.Width()) * anim);
				int height = (int) Math.Round(ShowCoords.Height() + (Height - ShowCoords.Height()) * anim);

				View.Alpha = anim;
				View.LayoutParameters = new RelativeLayout.LayoutParams(width, height) {
					LeftMargin = x,
					TopMargin = y
				};
			};
			CloseAnimator.AnimationEnd += delegate {
				CloseAnimator.Dispose();
				CloseAnimator = null;
				Context.OnDialogClosed(this);
			};
			CloseAnimator.SetDuration(200);
			CloseAnimator.Start();
		}

		public void ShowAt(int x, int y) {
			ShowCoords = new Rect(x, y, x, y);
			Show();
		}

		public void ShowAt(View view) {
			int[] coords = new int[2];
			view.GetLocationOnScreen(coords);
			int width = view.Width;
			int height = view.Height;
			ShowCoords = new Rect(coords[0], coords[1], coords[0] + width, coords[1] + height);
			Show();
		}

		#endregion

		public void Dispose() {
			View.Dispose();
			ContentView.Dispose();
			Buttons.Dispose();
			OpenAnimator?.Dispose();
			CloseAnimator?.Dispose();
			ShowCoords.Dispose();
		}
	}
}