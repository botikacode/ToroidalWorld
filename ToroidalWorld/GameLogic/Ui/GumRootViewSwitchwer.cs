using System;
using System.Collections.Generic;
using Gum.Forms.Controls;
using MonoGameGum;

namespace ToroidalWorld.GameLogic.Ui
{
    internal sealed class GumRootViewSwitcher
    {
        private readonly Dictionary<ScreenViewId, FrameworkElement> _views = new();

        public void Register(ScreenViewId id, FrameworkElement view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            _views[id] = view;
        }

        public void Show(ScreenViewId id)
        {
            GumService.Default.Root.Children.Clear();

            if (_views.TryGetValue(id, out var view) && view != null)
                view.AddToRoot();
        }
    }
}