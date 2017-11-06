<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/SimplePage.Master" AutoEventWireup="true" CodeBehind="ResourceNotFound.aspx.cs" Inherits="Hidistro.UI.Web.Admin.ResourceNotFound" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
 <div class="datalist">
		<div class="allerror mainwidth">
			<h2>系统处理错误，可能是：</h2>
			<div>
				1、您登录已经超时。<br />
				2、URL非法。<br />
				3、重复的操作，或者当前操作不可用。<br />
				<%--<a href="http://m.dfqm.cc/" target="_blank">以上都不是？请到官方网站寻求帮助！</a>--%>
			</div>
		</div>
	  </div>

	  <div class="blank12 clearfix"></div>
</asp:Content>

