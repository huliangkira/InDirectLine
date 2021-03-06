
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Extensions.Logging;

namespace Itminus.InDirectLine.IntegrationBotSample.Dialogs
{
    internal static class DialogNames
    {
        public static string TextPrompt = nameof(TextPrompt);
        public static string ChoicePrompt = nameof(ChoicePrompt);
        public static string Jokes = nameof(Jokes);
        public static string QuestionPurpose = nameof(QuestionPurpose);
    };

    public class MainDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserLocation> userLocationAccessor;
        private readonly ILogger<MainDialog> _logger;

        public MainDialog(IStatePropertyAccessor<UserLocation> userLocationAccessor ,ILogger<MainDialog> logger) : base(nameof(MainDialog))
        {
            this.userLocationAccessor = userLocationAccessor;
            this._logger = logger;


            AddDialog(new TextPrompt(DialogNames.TextPrompt));
            AddDialog(new ChoicePrompt(DialogNames.ChoicePrompt));
            AddDialog(new WaterfallDialog(DialogNames.Jokes, new WaterfallStep[]{
                // send background
                async(ctx , ct)=>{
                    var activity = MessageFactory.Text("有一天给你女朋友给你买了两条领带，一条蓝色的，一条粉色的。你很高兴，第二天一早，你打算穿一条到公司去。");
                    await ctx.Context.SendActivityAsync(activity,ct);
                    return await ctx.NextAsync(null,ct);
                },
                // prompt choice
                async(ctx , ct)=>{
                    var dialogTurnResult = await ctx.PromptAsync(DialogNames.ChoicePrompt,new PromptOptions{
                        Choices = new List<Choice>{
                            new Choice{
                                Value = "A: 蓝色的",
                                Synonyms = new List<string>{
                                    "A","蓝","蓝色","蓝色的",
                                },
                            },
                            new Choice{
                                Value = "B: 粉色的",
                                Synonyms = new List<string>{
                                    "B","粉","粉色","粉色的",
                                },
                            },
                        },
                        Style=ListStyle.SuggestedAction,
                    });
                    return dialogTurnResult;
                },
                // process
                async(ctx , ct)=>{
                    var choice = ctx.Result as FoundChoice;
                    var text = MessageFactory.Text($"你选择了{choice.Value}");
                    await ctx.Context.SendActivityAsync(text,ct);
                    var text2 = MessageFactory.Text("女朋友：你是不是不喜欢另外一条？");
                    await ctx.Context.SendActivityAsync(text2,ct);
                    var closing= MessageFactory.Text("笑话讲完了");
                    await ctx.Context.SendActivityAsync(closing,ct);
                    return await ctx.NextAsync(null,ct);
                },
            }));
            AddDialog(new WaterfallDialog(DialogNames.QuestionPurpose, new WaterfallStep[] {
                async(ctx, ct) =>{
                    var activity = MessageFactory.Text("目前我提供以下功能：");
                    await ctx.Context.SendActivityAsync(activity,ct);
                    return await ctx.NextAsync(null,ct);
                },
                async(ctx , ct)=>{
                    var dialogTurnResult = await ctx.PromptAsync(DialogNames.ChoicePrompt,new PromptOptions{
                        Choices = new List<Choice>{
                            new Choice{
                                Value = "A: 历史文章",
                                Synonyms = new List<string>{
                                    "A","历史","历史文章",
                                },
                            },
                            new Choice{
                                Value = "B: 笑话",
                                Synonyms = new List<string>{
                                    "B","笑话",
                                },
                            },
                        },
                        Style = ListStyle.HeroCard,
                    });
                    return dialogTurnResult;
                },
                // process
                async(ctx , ct)=>{
                    var choice = ctx.Result as FoundChoice;
                    if(choice.Index == 0){
                        var text = MessageFactory.Text($" 这里是{choice.Value}");
                        await ctx.Context.SendActivityAsync(text,ct);
                        return await ctx.NextAsync(null,ct);
                    } else if(choice.Index == 1) {
                        return await ctx.BeginDialogAsync(DialogNames.Jokes);
                    }else{
                        return await ctx.NextAsync(null,ct);
                    }
                },
            }));
            InitialDialogId = DialogNames.QuestionPurpose;
        }




    }

}