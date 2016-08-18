using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V17.Leanback.App;
using Android.Support.V17.Leanback.Widget;

namespace AndroidExample.TVLeanback.UI
{
    /// <summary>
    /// Activity that showcases different aspects of GuidedStepFragments.
    /// </summary>
    [Activity(Label = "GuidedStepActivity")]
    public class GuidedStepActivity : Activity
    {
        const int Continue = 0;
        const int Back = 1;
        const int OptionCheckSetId = 10;
        static readonly string[] OptionNames = { "Option A", "Option B", "Option C" };

        private static readonly string[] OptionDescriptions =
        {
            "Here's one thing you can do",
            "Here's another thing you can do",
            "Here's one more thing you can do"
        };

        private static readonly int[] OptionDrawables =
        {
            Resource.Drawable.ic_guidedstep_option_a,
            Resource.Drawable.ic_guidedstep_option_b,
            Resource.Drawable.ic_guidedstep_option_c
        };

        private static readonly bool[] OptionChecked = { true, false, false };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState == null)
                GuidedStepFragment.AddAsRoot(this, new FirstStepFragment(), Android.Resource.Id.Content);
            // Create your application here
        }

        static void AddAction(IList<GuidedAction> actions, long id, string title, string desc)
        {
            var action = (new GuidedAction.Builder()).Build();
            action.Id = id;
            action.Title = title;
            action.Description = desc;
            actions.Add(action);
        }

        static void AddCheckedAction(IList<GuidedAction> actions,
            int iconResId, Context context, string title, string desc, bool check)
        {
            var builder = new GuidedAction.Builder();
            builder = (GuidedAction.Builder)builder.CheckSetId(OptionCheckSetId);
            var guidedAction = builder.Build();
            guidedAction.Title = title;
            guidedAction.Description = desc;
            guidedAction.Icon = context.GetDrawable(iconResId);
            guidedAction.Checked = check;

            actions.Add(guidedAction);
        }

        class FirstStepFragment : GuidedStepFragment
        {
            public override int OnProvideTheme() => Resource.Style.Theme_Example_Leanback_GuidedStep_First;

            public override GuidanceStylist.Guidance OnCreateGuidance(Bundle savedInstanceState)
            {
                var title = "Guided Step First Page";
                var breadcrumb = "Guided Steps: 1";
                var description = "First step of guided sequence";
                var icon = Activity.GetDrawable(Resource.Drawable.ic_main_icon);
                return new GuidanceStylist.Guidance(title, description, breadcrumb, icon);
            }

            public override void OnCreateActions(IList<GuidedAction> actions, Bundle savedInstanceState)
            {
                AddAction(actions,
                    Continue,
                    "Continue",
                    "Let's do it");
                AddAction(actions,
                    Back,
                    "Cancel",
                    "Never mind");
            }

            public override void OnGuidedActionClicked(GuidedAction action)
            {
                var fm = FragmentManager;
                if (action.Id == Continue)
                {
                    Add(fm, new SecondStepFragment());
                }
                else
                {
                    Activity.FinishAfterTransition();
                }
            }
        }

        class SecondStepFragment : GuidedStepFragment
        {
            public override GuidanceStylist.Guidance OnCreateGuidance(Bundle savedInstanceState)
            {
                var title = "Guided Step Second Page";
                var breadcrumb = "Guided Steps: 2";
                var description = "Showcasing different action configurations";
                var icon = Activity.GetDrawable(Resource.Drawable.ic_main_icon);
                return new GuidanceStylist.Guidance(title, description, breadcrumb, icon);
            }

            public override GuidanceStylist OnCreateGuidanceStylist()
            {
                return new InnerGuidanceStylelist();
            }

            class InnerGuidanceStylelist : GuidanceStylist
            {
                public override int OnProvideLayoutId()
                {
                    return Resource.Layout.guidedstep_second_guidance;
                }
            }

            public override void OnCreateActions(IList<GuidedAction> actions, Bundle savedInstanceState)
            {
                var desc = "The description can be quite long as well. Just be sure...";
                var builder = new GuidedAction.Builder();
                builder =
                    (GuidedAction.Builder)builder.Title("Note that Guided Actions can have titles that are quite..");
                builder =
                    (GuidedAction.Builder)builder.Description(desc);
                builder =
                    (GuidedAction.Builder)builder.MultilineDescription(true);
                builder =
                    (GuidedAction.Builder)builder.InfoOnly(true);
                builder =
                    (GuidedAction.Builder)builder.Enabled(false);
                actions.Add(builder.Build());

                for (int i = 0; i < OptionNames.Length; ++i)
                {
                    AddCheckedAction(actions,
                        OptionDrawables[i],
                        Activity,
                        OptionNames[i],
                        OptionDescriptions[i],
                        OptionChecked[i]);
                }
            }

            public override void OnGuidedActionClicked(GuidedAction action)
            {
                var fm = FragmentManager;
                var next = ThirdStepFragment.NewInstance(SelectedActionPosition - 1);
                Add(fm, next);
            }
        }

        class ThirdStepFragment : GuidedStepFragment
        {
            static readonly string ArgOptionIdx = "arg.option.idx";

            public static ThirdStepFragment NewInstance(int option)
            {
                var f = new ThirdStepFragment();
                var args = new Bundle();
                args.PutInt(ArgOptionIdx, option);
                f.Arguments = args;
                return f;
            }

            public override GuidanceStylist.Guidance OnCreateGuidance(Bundle savedInstanceState)
            {
                var title = "Guided Step Third Page";
                var breadcrumb = "Guided Steps: 3";
                var description = "You chose:" + OptionNames[Arguments.GetInt(ArgOptionIdx)];
                var icon = Activity.GetDrawable(Resource.Drawable.ic_main_icon);
                return new GuidanceStylist.Guidance(title, description, breadcrumb, icon);
            }

            public override void OnCreateActions(IList<GuidedAction> actions, Bundle savedInstanceState)
            {
                AddAction(actions, Continue, "Done", "All finished");
                AddAction(actions, Back, "Back", "Forgot something...");
            }

            public override void OnGuidedActionClicked(GuidedAction action)
            {
                if (action.Id == Continue)
                {
                    Activity.FinishAfterTransition();
                }
                else
                {
                    FragmentManager.PopBackStack();
                }
            }
        }
    }
}