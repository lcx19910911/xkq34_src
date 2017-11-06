using ControlPanel.WeiBo;
using ControlPanel.WeiXin;
using Hidistro.ControlPanel.Store;
using Hidistro.ControlPanel.VShop;
using Hidistro.Core;
using Hidistro.Core.Entities;
using Hidistro.Entities.Members;
using Hidistro.Entities.Promotions;
using Hidistro.Entities.VShop;
using Hidistro.Entities.Weibo;
using Hidistro.SaleSystem.Vshop;
using Hishop.Weixin.MP;
using Hishop.Weixin.MP.Api;
using Hishop.Weixin.MP.Domain;
using Hishop.Weixin.MP.Handler;
using Hishop.Weixin.MP.Request;
using Hishop.Weixin.MP.Request.Event;
using Hishop.Weixin.MP.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace Hidistro.UI.Web.API
{
    public class CustomMsgHandler : RequestHandler
    {
        public CustomMsgHandler(Stream inputStream)
            : base(inputStream)
        {
        }

        private bool CreatMember(string OpenId, int ReferralUserId)
        {
            SiteSettings masterSettings = SettingsManager.GetMasterSettings(true);
            string tOKEN = TokenApi.GetToken_Message(masterSettings.WeixinAppId, masterSettings.WeixinAppSecret);
            string nickName = "";
            string headImageUrl = "";
            string retInfo = "";
            BarCodeApi.GetHeadImageUrlByOpenID(tOKEN, OpenId, out retInfo, out nickName, out headImageUrl);
            string generateId = Globals.GetGenerateId();
            MemberInfo info = new MemberInfo
            {
                GradeId = MemberProcessor.GetDefaultMemberGrade(),
                UserName = Globals.UrlDecode(nickName),
                OpenId = OpenId,
                CreateDate = DateTime.Now,
                SessionId = generateId,
                SessionEndTime = DateTime.Now.AddYears(10),
                UserHead = headImageUrl,
                ReferralUserId = ReferralUserId,
                Password = HiCryptographer.Md5Encrypt("888888")
            };
            Globals.Debuglog(JsonConvert.SerializeObject(info), "_Debuglog.txt");
            return MemberProcessor.CreateMember(info);
        }

        public override AbstractResponse DefaultResponse(AbstractRequest requestMessage)
        {
            WeiXinHelper.UpdateRencentOpenID(requestMessage.FromUserName);
            Hidistro.Entities.VShop.ReplyInfo mismatchReply = ReplyHelper.GetMismatchReply();
            if ((mismatchReply == null) || this.IsOpenManyService())
            {
                return this.GotoManyCustomerService(requestMessage);
            }
            AbstractResponse response = this.GetResponse(mismatchReply, requestMessage.FromUserName);
            if (response == null)
            {
                return this.GotoManyCustomerService(requestMessage);
            }
            response.ToUserName = requestMessage.FromUserName;
            response.FromUserName = requestMessage.ToUserName;
            return response;
        }

        private string FormatImgUrl(string img)
        {
            if (!img.StartsWith("http"))
            {
                img = string.Format("http://{0}{1}", HttpContext.Current.Request.Url.Host, img);
            }
            return img;
        }

        private AbstractResponse GetKeyResponse(string key, AbstractRequest request)
        {
            IList<Hidistro.Entities.VShop.ReplyInfo> replies = ReplyHelper.GetReplies(ReplyType.Vote);
            if ((replies != null) && (replies.Count > 0))
            {
                foreach (Hidistro.Entities.VShop.ReplyInfo info in replies)
                {
                    if (info.Keys == key)
                    {
                        VoteInfo voteById = StoreHelper.GetVoteById((long)info.ActivityId);
                        if ((voteById != null) && voteById.IsBackup)
                        {
                            NewsResponse response = new NewsResponse
                            {
                                CreateTime = DateTime.Now,
                                FromUserName = request.ToUserName,
                                ToUserName = request.FromUserName,
                                Articles = new List<Article>()
                            };
                            Article item = new Article
                            {
                                Description = voteById.VoteName,
                                PicUrl = this.FormatImgUrl(voteById.ImageUrl),
                                Title = voteById.VoteName,
                                Url = string.Format("http://{0}/vshop/Vote.aspx?voteId={1}", HttpContext.Current.Request.Url.Host, voteById.VoteId)
                            };
                            response.Articles.Add(item);
                            return response;
                        }
                    }
                }
            }
            return null;
        }

        public AbstractResponse GetResponse(Hidistro.Entities.VShop.ReplyInfo reply, string openId)
        {
            if (reply.MessageType == Hidistro.Entities.VShop.MessageType.Text)
            {
                TextReplyInfo info = reply as TextReplyInfo;
                TextResponse response = new TextResponse
                {
                    CreateTime = DateTime.Now,
                    Content = info.Text
                };
                if (reply.Keys == "登录")
                {
                    string str = string.Format("http://{0}/Vshop/Login.aspx?SessionId={1}", HttpContext.Current.Request.Url.Host, openId);
                    response.Content = response.Content.Replace("$login$", string.Format("<a href=\"{0}\">一键登录</a>", str));
                }
                return response;
            }
            NewsResponse response2 = new NewsResponse
            {
                CreateTime = DateTime.Now,
                Articles = new List<Article>()
            };
            if (reply.ArticleID > 0)
            {
                ArticleInfo articleInfo = ArticleHelper.GetArticleInfo(reply.ArticleID);
                if (articleInfo.ArticleType == ArticleType.News)
                {
                    Article item = new Article
                    {
                        Description = articleInfo.Memo,
                        PicUrl = this.FormatImgUrl(articleInfo.ImageUrl),
                        Title = articleInfo.Title,
                        Url = string.IsNullOrEmpty(articleInfo.Url) ? string.Format("http://{0}/Vshop/ArticleDetail.aspx?sid={1}", HttpContext.Current.Request.Url.Host, articleInfo.ArticleId) : articleInfo.Url
                    };
                    response2.Articles.Add(item);
                    return response2;
                }
                if (articleInfo.ArticleType == ArticleType.List)
                {
                    Article article3 = new Article
                    {
                        Description = articleInfo.Memo,
                        PicUrl = this.FormatImgUrl(articleInfo.ImageUrl),
                        Title = articleInfo.Title,
                        Url = string.IsNullOrEmpty(articleInfo.Url) ? string.Format("http://{0}/Vshop/ArticleDetail.aspx?sid={1}", HttpContext.Current.Request.Url.Host, articleInfo.ArticleId) : articleInfo.Url
                    };
                    response2.Articles.Add(article3);
                    foreach (ArticleItemsInfo info3 in articleInfo.ItemsInfo)
                    {
                        article3 = new Article
                        {
                            Description = "",
                            PicUrl = this.FormatImgUrl(info3.ImageUrl),
                            Title = info3.Title,
                            Url = string.IsNullOrEmpty(info3.Url) ? string.Format("http://{0}/Vshop/ArticleDetail.aspx?iid={1}", HttpContext.Current.Request.Url.Host, info3.Id) : info3.Url
                        };
                        response2.Articles.Add(article3);
                    }
                }
                return response2;
            }
            foreach (NewsMsgInfo info4 in (reply as NewsReplyInfo).NewsMsg)
            {
                Article article6 = new Article
                {
                    Description = info4.Description,
                    PicUrl = string.Format("http://{0}{1}", HttpContext.Current.Request.Url.Host, info4.PicUrl),
                    Title = info4.Title,
                    Url = string.IsNullOrEmpty(info4.Url) ? string.Format("http://{0}/Vshop/ImageTextDetails.aspx?messageId={1}", HttpContext.Current.Request.Url.Host, info4.Id) : info4.Url
                };
                response2.Articles.Add(article6);
            }
            return response2;
        }

        public AbstractResponse GotoManyCustomerService(AbstractRequest requestMessage)
        {
            WeiXinHelper.UpdateRencentOpenID(requestMessage.FromUserName);
            if (!this.IsOpenManyService())
            {
                return null;
            }
            return new AbstractResponse { FromUserName = requestMessage.ToUserName, ToUserName = requestMessage.FromUserName, MsgType = ResponseMsgType.transfer_customer_service };
        }

        public bool IsOpenManyService()
        {
            return SettingsManager.GetMasterSettings(false).OpenManyService;
        }

        public override AbstractResponse OnEvent_ClickRequest(ClickEventRequest clickEventRequest)
        {
            WeiXinHelper.UpdateRencentOpenID(clickEventRequest.FromUserName);
            Hidistro.Entities.VShop.MenuInfo menu = VShopHelper.GetMenu(Convert.ToInt32(clickEventRequest.EventKey));
            if (menu == null)
            {
                return null;
            }
            Hidistro.Entities.VShop.ReplyInfo reply = ReplyHelper.GetReply(menu.ReplyId);
            if (reply == null)
            {
                return null;
            }
            AbstractResponse keyResponse = this.GetKeyResponse(reply.Keys, clickEventRequest);
            if (keyResponse != null)
            {
                return keyResponse;
            }
            AbstractResponse response = this.GetResponse(reply, clickEventRequest.FromUserName);
            if (response == null)
            {
                this.GotoManyCustomerService(clickEventRequest);
            }
            response.ToUserName = clickEventRequest.FromUserName;
            response.FromUserName = clickEventRequest.ToUserName;
            return response;
        }

        public override AbstractResponse OnEvent_MassendJobFinishEventRequest(MassendJobFinishEventRequest massendJobFinishEventRequest)
        {
            string returnjsondata = string.Concat(new object[] { "公众号的微信号(加密的):", massendJobFinishEventRequest.ToUserName, ",发送完成时间：", massendJobFinishEventRequest.CreateTime, "，过滤通过条数：", massendJobFinishEventRequest.FilterCount, "，发送失败的粉丝数：", massendJobFinishEventRequest.ErrorCount });
            switch (massendJobFinishEventRequest.Status)
            {
                case "send success":
                    returnjsondata = returnjsondata + "(发送成功)";
                    break;

                case "send fail":
                    returnjsondata = returnjsondata + "(发送失败)";
                    break;

                case "err(10001)":
                    returnjsondata = returnjsondata + "(涉嫌广告)";
                    break;

                case "err(20001)":
                    returnjsondata = returnjsondata + "(涉嫌政治)";
                    break;

                case "err(20004)":
                    returnjsondata = returnjsondata + "(涉嫌社会)";
                    break;

                case "err(20002)":
                    returnjsondata = returnjsondata + "(涉嫌色情)";
                    break;

                case "err(20006)":
                    returnjsondata = returnjsondata + "(涉嫌违法犯罪)";
                    break;

                case "err(20008)":
                    returnjsondata = returnjsondata + "(涉嫌欺诈)";
                    break;

                case "err(20013)":
                    returnjsondata = returnjsondata + "(涉嫌版权)";
                    break;

                case "err(22000)":
                    returnjsondata = returnjsondata + "(涉嫌互相宣传)";
                    break;

                case "err(21000)":
                    returnjsondata = returnjsondata + "(涉嫌其他)";
                    break;

                default:
                    returnjsondata = returnjsondata + "(" + massendJobFinishEventRequest.Status + ")";
                    break;
            }
            WeiXinHelper.UpdateMsgId(0, massendJobFinishEventRequest.MsgId.ToString(), (massendJobFinishEventRequest.Status == "send success") ? 1 : 2, Globals.ToNum(massendJobFinishEventRequest.SentCount), Globals.ToNum(massendJobFinishEventRequest.TotalCount), returnjsondata);
            return null;
        }

        public override AbstractResponse OnEvent_ScanRequest(ScanEventRequest scanEventRequest)
        {
            string eventKey = scanEventRequest.EventKey;
            if (eventKey == "1")
            {
                if (WeiXinHelper.BindAdminOpenId.Count > 10)
                {
                    WeiXinHelper.BindAdminOpenId.Clear();
                }
                if (WeiXinHelper.BindAdminOpenId.ContainsKey(scanEventRequest.Ticket))
                {
                    WeiXinHelper.BindAdminOpenId[scanEventRequest.Ticket] = scanEventRequest.FromUserName;
                }
                else
                {
                    WeiXinHelper.BindAdminOpenId.Add(scanEventRequest.Ticket, scanEventRequest.FromUserName);
                }
                return new TextResponse { CreateTime = DateTime.Now, Content = "您正在扫描尝试绑定管理员身份，身份已识别", ToUserName = scanEventRequest.FromUserName, FromUserName = scanEventRequest.ToUserName };
            }
            ScanInfos scanInfosByTicket = ScanHelp.GetScanInfosByTicket(scanEventRequest.Ticket);
            Globals.Debuglog(eventKey + ":" + scanEventRequest.Ticket, "_Debuglog.txt");
            bool flag = MemberProcessor.IsExitOpenId(scanEventRequest.FromUserName);
            if ((!flag && (scanInfosByTicket != null)) && (scanInfosByTicket.BindUserId > 0))
            {
                this.CreatMember(scanEventRequest.FromUserName, scanInfosByTicket.BindUserId);
            }
            if (scanInfosByTicket != null)
            {
                ScanHelp.updateScanInfosLastActiveTime(DateTime.Now, scanInfosByTicket.Sceneid);
            }
            if (flag)
            {
                return new TextResponse { CreateTime = DateTime.Now, Content = "您刚扫描了分销商公众号二维码！", ToUserName = scanEventRequest.FromUserName, FromUserName = scanEventRequest.ToUserName };
            }
            Hidistro.Entities.VShop.ReplyInfo subscribeReply = ReplyHelper.GetSubscribeReply();
            if (subscribeReply == null)
            {
                return null;
            }
            subscribeReply.Keys = "扫描";
            AbstractResponse response = this.GetResponse(subscribeReply, scanEventRequest.FromUserName);
            response.ToUserName = scanEventRequest.FromUserName;
            response.FromUserName = scanEventRequest.ToUserName;
            return response;
        }

        public override AbstractResponse OnEvent_SubscribeRequest(SubscribeEventRequest subscribeEventRequest)
        {
            string eventKey = "";
            if (subscribeEventRequest.EventKey != null)
            {
                eventKey = subscribeEventRequest.EventKey;
            }
            if (eventKey.Contains("qrscene_"))
            {
                eventKey = eventKey.Replace("qrscene_", "").Trim();
                if (eventKey == "1")
                {
                    if (WeiXinHelper.BindAdminOpenId.Count > 10)
                    {
                        WeiXinHelper.BindAdminOpenId.Clear();
                    }
                    if (WeiXinHelper.BindAdminOpenId.ContainsKey(subscribeEventRequest.Ticket))
                    {
                        WeiXinHelper.BindAdminOpenId[subscribeEventRequest.Ticket] = subscribeEventRequest.FromUserName;
                    }
                    else
                    {
                        WeiXinHelper.BindAdminOpenId.Add(subscribeEventRequest.Ticket, subscribeEventRequest.FromUserName);
                    }
                    return new TextResponse { CreateTime = DateTime.Now, Content = "您正在扫描尝试绑定管理员身份，身份已识别", ToUserName = subscribeEventRequest.FromUserName, FromUserName = subscribeEventRequest.ToUserName };
                }
                ScanInfos scanInfosByTicket = ScanHelp.GetScanInfosByTicket(subscribeEventRequest.Ticket);
                Globals.Debuglog(eventKey + ":" + subscribeEventRequest.Ticket, "_Debuglog.txt");
                if ((!MemberProcessor.IsExitOpenId(subscribeEventRequest.FromUserName) && (scanInfosByTicket != null)) && (scanInfosByTicket.BindUserId > 0))
                {
                    this.CreatMember(subscribeEventRequest.FromUserName, scanInfosByTicket.BindUserId);
                    ScanHelp.updateScanInfosLastActiveTime(DateTime.Now, scanInfosByTicket.Sceneid);
                }
            }
            WeiXinHelper.UpdateRencentOpenID(subscribeEventRequest.FromUserName);
            Hidistro.Entities.VShop.ReplyInfo subscribeReply = ReplyHelper.GetSubscribeReply();
            if (subscribeReply == null)
            {
                return null;
            }
            subscribeReply.Keys = "登录";
            AbstractResponse response = this.GetResponse(subscribeReply, subscribeEventRequest.FromUserName);
            if (response == null)
            {
                this.GotoManyCustomerService(subscribeEventRequest);
            }
            response.ToUserName = subscribeEventRequest.FromUserName;
            response.FromUserName = subscribeEventRequest.ToUserName;
            return response;
        }

        public override AbstractResponse OnTextRequest(TextRequest textRequest)
        {
            WeiXinHelper.UpdateRencentOpenID(textRequest.FromUserName);
            AbstractResponse keyResponse = this.GetKeyResponse(textRequest.Content, textRequest);
            if (keyResponse != null)
            {
                return keyResponse;
            }
            IList<Hidistro.Entities.VShop.ReplyInfo> replies = ReplyHelper.GetReplies(ReplyType.Keys);
            if ((replies == null) || ((replies.Count == 0) && this.IsOpenManyService()))
            {
                this.GotoManyCustomerService(textRequest);
            }
            foreach (Hidistro.Entities.VShop.ReplyInfo info in replies)
            {
                if ((info.MatchType == MatchType.Equal) && (info.Keys == textRequest.Content))
                {
                    AbstractResponse response = this.GetResponse(info, textRequest.FromUserName);
                    response.ToUserName = textRequest.FromUserName;
                    response.FromUserName = textRequest.ToUserName;
                    return response;
                }
                if ((info.MatchType == MatchType.Like) && info.Keys.Contains(textRequest.Content))
                {
                    AbstractResponse response3 = this.GetResponse(info, textRequest.FromUserName);
                    response3.ToUserName = textRequest.FromUserName;
                    response3.FromUserName = textRequest.ToUserName;
                    return response3;
                }
            }
            return this.DefaultResponse(textRequest);
        }

        private void SaveLog(string LogInfo)
        {
            StreamWriter writer = File.AppendText(@"\Logty_Scan.txt");
            writer.WriteLine(LogInfo);
            writer.WriteLine(DateTime.Now);
            writer.Flush();
            writer.Close();
        }
    }
}

