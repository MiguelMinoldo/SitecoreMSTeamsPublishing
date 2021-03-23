using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MessageCardModel;
using MessageCardModel.Actions;
using MessageCardModel.Actions.OpenUri;
using Microsoft.Extensions.DependencyInjection;
using MSTeamsPublishing.Services;
using Sitecore.DependencyInjection;
using Sitecore.Publishing;
using Sitecore.Sites;

namespace MSTeamsPublishing.Events
{
    public class Notification
    {
        private readonly IItemSiteResolver _siteResolver;
        private readonly IMsTeamsConnectorService _msTeamsConnectorService;

        public Notification()
        {
            _siteResolver = ServiceLocator.ServiceProvider.GetService<IItemSiteResolver>();
            _msTeamsConnectorService = ServiceLocator.ServiceProvider.GetService<IMsTeamsConnectorService>();
        }

        public Notification(IItemSiteResolver siteResolver, IMsTeamsConnectorService msTeamsConnectorService)
        {
            _siteResolver = siteResolver;
            _msTeamsConnectorService = msTeamsConnectorService;
        }

        public void SendNotification(object sender, EventArgs args)
        {
            var sitecoreArgs = args as Sitecore.Events.SitecoreEventArgs;

            if (!(sitecoreArgs?.Parameters[0] is Publisher publisher)) return;

            var rootItem = publisher.Options.RootItem;
            var publishJobs = Sitecore.Jobs.JobManager.GetJobs().Where(x => x.Name.Equals(publisher.GetJobName())).ToList();
            var site = _siteResolver.ResolveSite(rootItem);
            var hostUrl = "https://" + (site != null ? site.HostName : $"{HttpContext.Current?.Request.Url.Scheme}://{HttpContext.Current?.Request.Url.Host}");
            var ItemId = HttpUtility.UrlEncode(rootItem.ID.ToString());

            foreach (var j in publishJobs.Where(p => p.Handle.IsLocal))
            {
                var teamsMessage = new MessageCard();
                var facts = new List<Fact> { new Fact {Name = "User: ", Value = publisher.Options.UserName } };

                foreach (var message in j.Status.Messages)
                {
                    var messageSplit = message.Split(':');
                    var fact = new Fact {Name = $"{messageSplit[0]}: ", Value = messageSplit[1]};
                    facts.Add(fact);
                }

                var section = new Section
                {
                    ActivityTitle = $"{j.Name} Done!",
                    ActivitySubtitle = $"Version: {rootItem.Version}, Language: {rootItem.Language}, Target DB: {publisher.Options.TargetDatabase}. Subitems: {publisher.Options.Deep}",
                    ActivityImage = "https://sitecorecdn.azureedge.net/-/media/sitecoresite/images/global/logo/favicon.png",
                    Facts = facts
                };

                var sitecoreRedirectAction = new OpenUriAction { Type = ActionType.OpenUri, Name = "Go to Sitecore", Targets = new [] { new Target { OS = TargetOs.Default, Uri = $"{hostUrl}/sitecore/shell/sitecore/content/Applications/Content Editor.aspx?id={ItemId}&amp;la={rootItem.Language}&amp;fo={ItemId}" } } };
                var publicRedirectAction = new OpenUriAction { Type = ActionType.OpenUri, Name = "Go to website", Targets = new [] { new Target { OS = TargetOs.Default, Uri = $"{hostUrl}/?sc_itemid={ItemId}&amp;sc_mode=normal&amp;sc_lang={rootItem.Language}" } } };

                teamsMessage.Context = "https://schema.org/extensions";
                teamsMessage.Type = "MessageCard";
                teamsMessage.Summary = "Publish Notification";
                teamsMessage.ThemeColor = "008000";
                teamsMessage.Sections = new [] {section};
                teamsMessage.Actions = new [] {sitecoreRedirectAction, publicRedirectAction};

                _msTeamsConnectorService.ProcessAsync(teamsMessage).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}