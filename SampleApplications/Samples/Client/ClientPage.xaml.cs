/* Copyright (c) 1996-2016, OPC Foundation. All rights reserved.
   The source code in this file is covered under a dual-license scenario:
     - RCL: for OPC Foundation members in good-standing
     - GPL V2: everybody else
   RCL license terms accompanied with this source code. See http://opcfoundation.org/License/RCL/1.00/
   GNU General Public License as published by the Free Software Foundation;
   version 2 of the License are accompanied with this source code. See http://opcfoundation.org/License/GPLv2
   This source code is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
*/

using Opc.Ua.Client;
using Opc.Ua.Client.Controls;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Text;
using Windows.UI;
using System.Threading;
using Opc.Ua.SampleClient.Models;
using System.Collections.ObjectModel;
using System.Text;

namespace Opc.Ua.SampleClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class UaClientItems
    {
        public Session                          m_session;
        public HistoryReadResult                m_result;
        public ObservableCollection<ItemModel>  results             = new ObservableCollection<ItemModel>();
        public ComboBox                         comboBox            = null;
        public uint                             SizeHistoricItems   = 10;
    }

    public partial class ClientPage : Page
    {
        #region Private Fields
        UaClientItems                           uaCItems    = new UaClientItems();
        private ApplicationInstance             m_application;
        private Opc.Ua.Server.StandardServer    m_server;
        private ConfiguredEndpointCollection    m_endpoints;
        private ApplicationConfiguration        m_configuration;
        private ServiceMessageContext           m_context;
        private ClientPage                      m_masterPage;
        private List<ClientPage>                m_pages;
        private List<ItemModel>[]         ClientHandleList  = new List<ItemModel>[512];
        private SortedDictionary<string, List<ItemModel>> nodesList = new SortedDictionary<string, List<ItemModel>>();
        private String[] namespaces                         = null;
        #endregion

        public ClientPage()
        {
            InitializeComponent();
        }
        #region
        /// <summary>
        /// Formats the value of an attribute.
        /// </summary>
        private string FormatAttributeValue(uint attributeId, object value)
        {
            switch (attributeId)
            {
                case Attributes.NodeClass:
                    {
                        if (value != null)
                        {
                            return String.Format("{0}", Enum.ToObject(typeof(NodeClass), value));
                        }

                        return "(null)";
                    }

                case Attributes.DataType:
                    {
                        NodeId datatypeId = value as NodeId;

                        if (datatypeId != null)
                        {
                            INode datatype = uaCItems.m_session.NodeCache.Find(datatypeId);

                            if (datatype != null)
                            {
                                return String.Format("{0}", datatype.DisplayName.Text);
                            }
                            else
                            {
                                return String.Format("{0}", datatypeId);
                            }
                        }

                        return String.Format("{0}", value);
                    }

                case Attributes.ValueRank:
                    {
                        int? valueRank = value as int?;

                        if (valueRank != null)
                        {
                            switch (valueRank.Value)
                            {
                                case ValueRanks.Scalar: return "";
                                case ValueRanks.OneDimension: return "[]";
                                case ValueRanks.OneOrMoreDimensions: return "[][]";
                                case ValueRanks.Any: return "Any";

                                default:
                                    {
                                        return String.Format("{0}", valueRank.Value);
                                    }
                            }
                        }

                        return String.Format("{0}", value);
                    }

                case Attributes.MinimumSamplingInterval:
                    {
                        double? minimumSamplingInterval = value as double?;

                        if (minimumSamplingInterval != null)
                        {
                            if (minimumSamplingInterval.Value == MinimumSamplingIntervals.Indeterminate)
                            {
                                return "Indeterminate";
                            }

                            else if (minimumSamplingInterval.Value == MinimumSamplingIntervals.Continuous)
                            {
                                return "Continuous";
                            }

                            return String.Format("{0}", minimumSamplingInterval.Value);
                        }

                        return String.Format("{0}", value);
                    }

                case Attributes.AccessLevel:
                case Attributes.UserAccessLevel:
                    {
                        byte accessLevel = Convert.ToByte(value);

                        StringBuilder bits = new StringBuilder();

                        if ((accessLevel & AccessLevels.CurrentRead) != 0)
                        {
                            bits.Append("R");
                        }

                        if ((accessLevel & AccessLevels.CurrentWrite) != 0)
                        {
                            if (bits.Length > 0)
                            {
                                bits.Append(" | ");
                            }

                            bits.Append("W");
                        }

                        if ((accessLevel & AccessLevels.HistoryRead) != 0)
                        {
                            if (bits.Length > 0)
                            {
                                bits.Append(" | ");
                            }

                            bits.Append("H");
                        }

                        if ((accessLevel & AccessLevels.HistoryWrite) != 0)
                        {
                            if (bits.Length > 0)
                            {
                                bits.Append(" | ");
                            }

                            bits.Append("HU");
                        }

                        if (bits.Length == 0)
                        {
                            bits.Append("No Access");
                        }

                        return String.Format("{0}", bits);
                    }

                case Attributes.EventNotifier:
                    {
                        byte notifier = Convert.ToByte(value);

                        StringBuilder bits = new StringBuilder();

                        if ((notifier & EventNotifiers.SubscribeToEvents) != 0)
                        {
                            bits.Append("Subscribe");
                        }

                        if ((notifier & EventNotifiers.HistoryRead) != 0)
                        {
                            if (bits.Length > 0)
                            {
                                bits.Append(" | ");
                            }

                            bits.Append("History");
                        }

                        if ((notifier & EventNotifiers.HistoryWrite) != 0)
                        {
                            if (bits.Length > 0)
                            {
                                bits.Append(" | ");
                            }

                            bits.Append("History Update");
                        }

                        if (bits.Length == 0)
                        {
                            bits.Append("No Access");
                        }

                        return String.Format("{0}", bits);
                    }

                default:
                    {
                        return String.Format("{0}", value);
                    }
            }
        }
        #endregion
        private ushort namespaceIndex(String s)
        {
            ushort ret = 0;
            foreach( var str in namespaces)
            {
                if (s == str)
                {
                    break;
                }
                ret++;
            }
            return ret;
        }

        public ClientPage(
           ServiceMessageContext context,
           ApplicationInstance application,
           ClientPage masterPage,
           ApplicationConfiguration configuration)
        {
            InitializeComponent();

            if (!configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
            {
                configuration.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
            }

            m_masterPage = masterPage;
            m_context = context;
            m_application = application;
            m_server = application.Server as Opc.Ua.Server.StandardServer;

            if (m_masterPage == null)
            {
                m_pages = new List<ClientPage>();
            }

            m_configuration = configuration;

            SessionsCTRL.Configuration = configuration;
            SessionsCTRL.MessageContext = context;
            SessionsCTRL.AddressSpaceCtrl = BrowseCTRL;
            SessionsCTRL.NodeSelected += SessionCtrl_NodeSelected;

            // get list of cached endpoints.
            m_endpoints = m_configuration.LoadCachedEndpoints(true);
            m_endpoints.DiscoveryUrls = configuration.ClientConfiguration.WellKnownDiscoveryUrls;

            // hook up endpoint selector
            EndpointSelectorCTRL.Initialize(m_endpoints, m_configuration);
            EndpointSelectorCTRL.ConnectEndpoint += EndpointSelectorCTRL_ConnectEndpoint;
            EndpointSelectorCTRL.EndpointsChanged += EndpointSelectorCTRL_OnChange;

            BrowseCTRL.SessionTreeCtrl = SessionsCTRL;
            BrowseCTRL.NodeSelected += BrowseCTRL_NodeSelected;


            // exception dialog
            GuiUtils.ExceptionMessageDlg += ExceptionMessageDlg;

            ServerUrlTB.Text = "None";
        }

        void RemoveAllClickEventsFromButton()
        {
            CommandBTN.Click -= ContextMenu_OnDelete;
            CommandBTN.Click -= ContextMenu_OnCancel;
            CommandBTN.Click -= ContextMenu_OnDisconnect;
            CommandBTN.Click -= ContextMenu_OnReport;
        }

        private void SessionCtrl_NodeSelected(object sender, TreeNodeActionEventArgs e)
        {
            if (e.Node != null)
            {
                MonitoredItem item = e.Node as MonitoredItem;
                if (e.Node is MonitoredItem)
                {
                    BrowseCTRL.Clear();
                    CommandBTN.Visibility = Visibility.Visible;
                    CommandBTN.Content = "Delete";
                    RemoveAllClickEventsFromButton();
                    CommandBTN.Click += ContextMenu_OnDelete;
                    CommandBTN.Tag = e.Node;
                }
                else if (e.Node is Subscription)
                {
                    BrowseCTRL.Clear();
                    CommandBTN.Visibility = Visibility.Visible;
                    CommandBTN.Content = "Cancel";
                    RemoveAllClickEventsFromButton();
                    CommandBTN.Click += ContextMenu_OnCancel;
                    CommandBTN.Tag = e.Node;
                }
                else if (e.Node is Session)
                {
                    BrowseCTRL.Clear();
                    CommandBTN.Visibility   = Visibility.Visible;
                    CommandBTN.Content      = "Disconnect";
                    RemoveAllClickEventsFromButton();
                    CommandBTN.Click        += ContextMenu_OnDisconnect;
                    CommandBTN.Tag          = e.Node;

                    // Update current session object
                    uaCItems.m_session = (Session)e.Node;

                    if (namespaces == null)
                    {
                        DataValue ns = uaCItems.m_session.ReadValue(new NodeId(Variables.Server_NamespaceArray, 0));
                        if (ns.Value is Array)
                        {
                            var arr = ns.Value as Array;
                            namespaces = new string[arr.Length];
                            int index = 0;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                namespaces[i] = (string)(arr.GetValue(i));
                                Namespace.Items.Add(namespaces[i]);
                                if (namespaces[i] == "S7:") index = i;
                            }
                            if (Namespace.Items.Count > 0) Namespace.SelectedIndex = index;
                        }

                        NamespaceLabel.Visibility   = Visibility.Visible;
                        Namespace.Visibility        = Visibility.Visible;
                        IdentifierLabel.Visibility  = Visibility.Visible;
                        Identifier.Visibility       = Visibility.Visible;
                        AddNodeBTN.Visibility       = Visibility.Visible;
                        ServerUrlTB.Visibility      = Visibility.Visible;
                        ServerStatusTB.Visibility   = Visibility.Visible;

                    }
                }
                else
                {
                    RemoveAllClickEventsFromButton();
                    CommandBTN.Visibility = Visibility.Collapsed;
                    CommandBTN.Tag = null;
                }
            }
        }

        private void BrowseCTRL_NodeSelected(object sender, TreeNodeActionEventArgs e)
        {
            if (e.Node != null)
            {
                ReferenceDescription reference = e.Node as ReferenceDescription;
                if (reference != null && reference.NodeClass == NodeClass.Variable)
                {
                    CommandBTN.Visibility = Visibility.Visible;
                    CommandBTN.Content = "Report";
                    RemoveAllClickEventsFromButton();
                    CommandBTN.Click += ContextMenu_OnReport;
                    CommandBTN.Tag = e.Node;
                }
                else
                {
                    CommandBTN.Visibility = Visibility.Visible;
                    CommandBTN.Content = "Disconnect";
                    RemoveAllClickEventsFromButton();
                    CommandBTN.Click += ContextMenu_OnDisconnect;
                    CommandBTN.Tag = uaCItems.m_session;
                }
            }
        }

        private void ContextMenu_OnDisconnect(object sender, RoutedEventArgs e)
        {
            try
            {
                SessionsCTRL.Delete(CommandBTN.Tag as Session);
                RemoveAllClickEventsFromButton();
                CommandBTN.Visibility       = Visibility.Collapsed;
                NamespaceLabel.Visibility   = Visibility.Collapsed;
                Namespace.Visibility        = Visibility.Collapsed;
                IdentifierLabel.Visibility  = Visibility.Collapsed;
                Identifier.Visibility       = Visibility.Collapsed;
                AddNodeBTN.Visibility       = Visibility.Collapsed;
                ServerUrlTB.Visibility      = Visibility.Collapsed;
                ServerStatusTB.Visibility   = Visibility.Collapsed;
                namespaces                  = null;
                this.listViewDesktop.Visibility     = Visibility.Collapsed;
                this.listViewMobile.Visibility      = Visibility.Collapsed;
                this.listViewDesktop.DataContext    = null;
                this.listViewMobile.DataContext     = null;
                BrowseCTRL.Clear();
                ClientHandleList                    = new List<ItemModel>[512];
                nodesList                           = new SortedDictionary<string, List<ItemModel>>();
                uaCItems.results.Clear();
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        private void ContextMenu_OnCancel(object sender, RoutedEventArgs e)
        {
            try
            {
                lock (uaCItems.results)
                {
                    SessionsCTRL.Delete(CommandBTN.Tag as Subscription);
                    ClientHandleList                = new List<ItemModel>[512];
                    nodesList                       = new SortedDictionary<string, List<ItemModel>>();
                    uaCItems.results.Clear();
                    if (App.IsMobile)
                    {
                        uaCItems.SizeHistoricItems = 5;
                        this.listViewMobile.Visibility = Visibility.Visible;
                        this.listViewDesktop.Visibility = Visibility.Collapsed;
                        this.listViewMobile.DataContext = uaCItems.results;
                    }
                    else
                    {
                        uaCItems.SizeHistoricItems = 10;
                        this.listViewDesktop.Visibility = Visibility.Visible;
                        this.listViewMobile.Visibility = Visibility.Collapsed;
                        this.listViewDesktop.DataContext = uaCItems.results;
                    }
                }
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        private void ContextMenu_OnDelete(object sender, RoutedEventArgs e)
        {

            try
            {
                var monitoredItem = CommandBTN.Tag as MonitoredItem;
                if (monitoredItem == null) return;
                var subscription                    = monitoredItem.Subscription;
                SessionsCTRL.Delete(monitoredItem);
                if (subscription.MonitoredItemCount == 0)
                {
                    // Remove subscription if no more items
                    CommandBTN.Tag = subscription;
                    ContextMenu_OnCancel(sender, e);
                }

                lock (uaCItems.results)
                {
                    List<ItemModel> list = ClientHandleList[(int)monitoredItem.ClientHandle];
                    if (list != null)
                    {
                        ItemModel iT        = null;
                        foreach (var it in list)
                        {
                            if (it.ClientHandle == monitoredItem.ClientHandle)
                            {
                                iT = it;
                                if (uaCItems.results.Contains(it)) uaCItems.results.Remove(it);
                            }
                        }
                        ClientHandleList[(int)monitoredItem.ClientHandle] = null;
                        if (uaCItems.results.Count < 2)
                        {
                            ClientHandleList    = new List<ItemModel>[512];
                            nodesList           = new SortedDictionary<string, List<ItemModel>>();
                            uaCItems.results.Clear();
                        }

                        if (iT != null)
                        {
                            if (nodesList.ContainsKey(iT.NodeId.ToString()))
                            {
                                var l = nodesList[iT.NodeId.ToString()];
                                l.Remove(iT);
                                if (l.Count == 0) nodesList.Remove(iT.NodeId.ToString());
                            }

                            if (App.IsMobile)
                            {
                                uaCItems.SizeHistoricItems = 5;
                                this.listViewMobile.Visibility = Visibility.Visible;
                                this.listViewDesktop.Visibility = Visibility.Collapsed;
                                this.listViewMobile.DataContext = uaCItems.results;
                            }
                            else
                            {
                                uaCItems.SizeHistoricItems = 5;
                                this.listViewDesktop.Visibility = Visibility.Visible;
                                this.listViewMobile.Visibility = Visibility.Collapsed;
                                this.listViewDesktop.DataContext = uaCItems.results;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        async private void AddNodeBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
            if (uaCItems.m_session != null)
                {
                    NodeId node = new NodeId(Identifier.Text, (ushort)Namespace.SelectedIndex);
                    await CreateMonitoredItem(
                        uaCItems.m_session, null, node, MonitoringMode.Reporting);
                }
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        async private void ContextMenu_OnReport(object sender, RoutedEventArgs e)
        {
            try
            {
                // can only subscribe to local variables. 
                ReferenceDescription reference = CommandBTN.Tag as ReferenceDescription;
                if (uaCItems.m_session != null && reference != null)
                {
                    await CreateMonitoredItem(
                        uaCItems.m_session, null, (NodeId)reference.NodeId, MonitoringMode.Reporting);
                }
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            ManualResetEvent ev = new ManualResetEvent(false);
            Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    await GuiUtils.HandleCertificateValidationError(this, validator, e);
                    ev.Set();
                }
                ).AsTask().Wait();
            ev.WaitOne();
        }

        public void OpenPage()
        {
            if (m_masterPage == null)
            {
                ClientPage page = new ClientPage(m_context, m_application, this, m_configuration);
                m_pages.Add(page);
                page.Unloaded += Window_PageClosing;
            }
            else
            {
                m_masterPage.OpenPage();
            }
        }

        async void Window_PageClosing(object sender, RoutedEventArgs e)
        {
            if (m_masterPage == null && m_pages.Count > 0)
            {
                MessageDlg dialog = new MessageDlg("Close all sessions?", MessageDlgButton.Yes, MessageDlgButton.No);
                MessageDlgButton result = await dialog.ShowAsync();
                if (result != MessageDlgButton.Yes)
                {
                    return;
                }
            }

            BrowseCTRL.Clear();

            for (int ii = 0; ii < m_pages.Count; ii++)
            {
                if (Object.ReferenceEquals(m_pages[ii], sender))
                {
                    m_pages.RemoveAt(ii);
                    break;
                }
            }
        }

        /// <summary>
        /// Provides a user defined method.
        /// </summary>
        protected virtual async void DoTest(Session session)
        {
            MessageDlg dialog = new MessageDlg("A handy place to put test code.");
            await dialog.ShowAsync();
        }

        async Task EndpointSelectorCTRL_ConnectEndpoint(object sender, ConnectEndpointEventArgs e)
        {
            try
            {
                // disable Connect while connecting button
                EndpointSelectorCTRL.IsEnabled = false;
                // Connect
                e.UpdateControl = await Connect(e.Endpoint);
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
                e.UpdateControl = false;
            }
            finally
            {
                // enable Connect button
                EndpointSelectorCTRL.IsEnabled = true;
            }
        }

        private void EndpointSelectorCTRL_OnChange(object sender, EventArgs e)
        {
            try
            {
                m_endpoints.Save();
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        /// <summary>
        /// Connects to a server.
        /// </summary>
        public async Task<bool> Connect(ConfiguredEndpoint endpoint)
        {
            bool result = false;
            if (endpoint == null)
            {
                return false;
            }

            // connect dialogs
            Session session = await SessionsCTRL.Connect(endpoint);

            if (session != null)
            {
                //hook up new session
                session.KeepAlive += new KeepAliveEventHandler(StandardClient_KeepAlive);
                StandardClient_KeepAlive(session, null);

                // BrowseCTRL.SetView(session, BrowseViewType.Objects, null);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Updates the status control when a keep alive event occurs.
        /// </summary>
        async void StandardClient_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (!Dispatcher.HasThreadAccess)
            {
                await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                () =>
                {
                    StandardClient_KeepAlive(sender, e);
                });
                return;
            }

            if (sender != null && sender.Endpoint != null)
            {
                ServerUrlTB.Text = Utils.Format(
                    "{0} ({1}) {2}",
                    sender.Endpoint.EndpointUrl,
                    sender.Endpoint.SecurityMode,
                    (sender.EndpointConfiguration.UseBinaryEncoding) ? "UABinary" : "XML");
            }
            else
            {
                ServerUrlTB.Text = "None";
            }

            if (e != null && uaCItems.m_session != null)
            {
                SessionsCTRL.UpdateSessionNode(uaCItems.m_session);

                if (ServiceResult.IsGood(e.Status))
                {
                    ServerStatusTB.Text = Utils.Format(
                        "Server Status: {0} {1:yyyy-MM-dd HH:mm:ss} {2}/{3}",
                        e.CurrentState,
                        e.CurrentTime.ToLocalTime(),
                        uaCItems.m_session.OutstandingRequestCount,
                        uaCItems.m_session.DefunctRequestCount);
                    ServerStatusTB.Foreground = ItemModel.solidColorBrushBlack;
                    ServerStatusTB.FontWeight = FontWeights.Normal;
                }
                else
                {
                    foreach (ItemModel itemModel in uaCItems.results)
                    {
                        itemModel.SolidColorBrush = new SolidColorBrush(Colors.Red);
                        itemModel.StatusCodeStr = e.Status.StatusCode.ToString();
                    }
                    ServerStatusTB.Text = String.Format(
                        "{0} {1}/{2}", e.Status,
                        uaCItems.m_session.OutstandingRequestCount,
                        uaCItems.m_session.DefunctRequestCount);
                    ServerStatusTB.Foreground = ItemModel.solidColorBrushRed;
                    ServerStatusTB.FontWeight = FontWeights.Bold;
                }
            }
        }

        async void ExceptionMessageDlg(string message)
        {
            await Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal,
                async () =>
            {
                MessageDlg dialog = new MessageDlg(message);
                await dialog.ShowAsync();
            });
        }

        private void MainPage_PageClosing(object sender, RoutedEventArgs e)
        {
            try
            {
                SessionsCTRL.Close();

                if (m_masterPage == null)
                {
                    m_application.Stop();
                }
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        private void DiscoverServersMI_Click(object sender, EventArgs e)
        {
            try
            {
                ConfiguredEndpoint endpoint = new ConfiguredServerListDlg().ShowDialog(m_configuration, true);

                if (endpoint != null)
                {
                    EndpointSelectorCTRL.SelectedEndpoint = endpoint;
                    return;
                }
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        private void NewWindowMI_Click(object sender, EventArgs e)
        {
            try
            {
                this.OpenPage();
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        private void Discovery_RegisterMI_Click(object sender, EventArgs e)
        {
            try
            {
                if (m_server != null)
                {
                    OnRegister(null);
                }
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        private async void OnRegister(object sender)
        {
            try
            {
                Opc.Ua.Server.StandardServer server = m_server;

                if (server != null)
                {
                    await server.RegisterWithDiscoveryServer();
                }
            }
            catch (Exception exception)
            {
                Utils.Trace(exception, "Could not register with the LDS");
            }
        }

        private void BrowseNodeId(NodeId nodeId, bool includeSubtypes)
        {
            BrowseDescriptionCollection browseDescriptionCollection = new BrowseDescriptionCollection();
            DiagnosticInfoCollection diagnosticInfos = null;
            var bd = new BrowseDescription();
            bd.NodeId = nodeId;
            bd.IncludeSubtypes = includeSubtypes;
            browseDescriptionCollection.Add(bd);
            BrowseResultCollection browseResultCollection = null;
            uaCItems.m_session.Browse(null, null, 10, browseDescriptionCollection, out browseResultCollection, out diagnosticInfos);
        }

        private async Task<Subscription> CreateMonitoredItemTask(
           Session session, Subscription subscription, NodeId nodeId, MonitoringMode mode, List<Grid> list, ItemModel itemModel)
        {
            // Create subscription if it does'nt exist
            if (subscription == null)
            {
                subscription = session.DefaultSubscription;
                if (session.AddSubscription(subscription))
                    subscription.Create();
            }
            else
            {
                session.AddSubscription(subscription);
            }

            // add the new monitored item.
            MonitoredItem monitoredItem = new MonitoredItem(subscription.DefaultItem);
            // BrowseNodeId(nodeId, false);

            // Get item attributes
            SortedDictionary<uint, int> attributes = new SortedDictionary<uint, int>();
            attributes.Add(Attributes.DisplayName, 0);
            attributes.Add(Attributes.DataType, 1);
            attributes.Add(Attributes.ValueRank, 2);
            attributes.Add(Attributes.AccessLevel, 3);

            // build list of values to read.
            ReadValueIdCollection itemsToRead = new ReadValueIdCollection();

            foreach (uint attributeId in attributes.Keys)
            {
                ReadValueId itemToRead = new ReadValueId();

                itemToRead.NodeId       = nodeId;
                itemToRead.AttributeId  = attributeId;
                itemsToRead.Add(itemToRead);
            }

            // read from server.
            DataValueCollection     attributesValues = null;
            DiagnosticInfoCollection diagnosticInfos = null;
            diagnosticInfos = null;

            ResponseHeader responseHeader = session.Read(
                null,
                0,
                TimestampsToReturn.Neither,
                itemsToRead,
                out attributesValues,
                out diagnosticInfos);

            // Get type name
            NodeId typeNodeId = attributesValues[attributes[Attributes.DataType]].Value as NodeId;
            List<NodeId> listNodeId = new List<NodeId>();
            listNodeId.Add(typeNodeId);
            List<String> typeList = null;
            List<ServiceResult> listServiceResult = null;
            session.ReadDisplayName(listNodeId, out typeList, out listServiceResult);

            String ValueRank                = FormatAttributeValue(Attributes.ValueRank, attributesValues[attributes[Attributes.ValueRank]].Value);
            String AccessLevel              = FormatAttributeValue(Attributes.AccessLevel, attributesValues[attributes[Attributes.AccessLevel]].Value);

            monitoredItem.StartNodeId       = nodeId;
            monitoredItem.AttributeId       = Attributes.Value;

            monitoredItem.DisplayName       = attributesValues[attributes[Attributes.DisplayName]].ToString();
            monitoredItem.MonitoringMode    = mode;
            monitoredItem.SamplingInterval  = mode == MonitoringMode.Sampling ? 1000 : 0;
            monitoredItem.QueueSize         = 0;
            monitoredItem.DiscardOldest     = true;

            monitoredItem.Notification += new MonitoredItemNotificationEventHandler(MonitoredItem_Notification);
            itemModel.BuiltInType       = TypeInfo.GetBuiltInType(typeNodeId, session.TypeTree);

            itemModel.SolidColorBrush   = ItemModel.solidColorBrushRed;
            itemModel.StatusCodeStr     = "BadWaitingForInitialData";
            itemModel.AccessLevel       = AccessLevel;
            if (typeList.Count == 1) itemModel.Type = typeList[0] + ValueRank;
            else itemModel.Type         = String.Empty;
            itemModel.DisplayName       = monitoredItem.DisplayName;
            itemModel.ClientHandle      = monitoredItem.ClientHandle;
            itemModel.DisplayName       = monitoredItem.DisplayName;

            itemModel.headerVisibility  = Visibility.Collapsed;
            itemModel.restVisibility    = Visibility.Visible;

            itemModel.NodeId = nodeId;
            itemModel.NodeIdStr = nodeId.ToString();
            list.Add(Util.generateGridNodeId("NodeId", itemModel.NodeIdStr));
            list.Add(Util.generateGridNodeId("DisplayName", itemModel.DisplayName));
            if (nodeId.NamespaceIndex <= namespaces.Length) list.Add(Util.generateGridNodeId("Namespace", namespaces[nodeId.NamespaceIndex]));
            list.Add(Util.generateGridNodeId("AccessLevel", itemModel.AccessLevel));
            list.Add(Util.generateGridNodeId("Type", itemModel.Type));

            subscription.AddItem(monitoredItem);
            subscription.ApplyChanges();

            return subscription;
        }

        async public Task CreateMonitoredItem(
           Session session, Subscription subscription, NodeId nodeId, MonitoringMode mode)
        {
            var list            = new List<Grid>();
            ItemModel itemModel = new ItemModel(uaCItems);
            itemModel.Changes   = new List<ItemModel>((int)(uaCItems.SizeHistoricItems + 1));
            var task            = CreateMonitoredItemTask(session, subscription, nodeId, mode, list, itemModel) as Task<Subscription>;
            task.Wait(500);
            subscription   = task.Result;

            if (task.IsCompleted)
            {
                lock (uaCItems.results)
                {
                    if (uaCItems.results.Count == 0)
                    {
                        var headerItemModel         = new ItemModel(uaCItems);
                        headerItemModel.AccessLevel = "AccessLevel";
                        headerItemModel.NodeIdStr   = "NodeId";
                        headerItemModel.DisplayName = "DisplayName";
                        headerItemModel.Value       = "Value";
                        headerItemModel.StatusCodeStr = "StatusCode";
                        headerItemModel.Timestamp   = "Timestamp";
                        headerItemModel.Type        = "Type";
                        headerItemModel.headerVisibility = Visibility.Visible;
                        headerItemModel.restVisibility  = Visibility.Collapsed;
                        uaCItems.results.Add(headerItemModel);
                    }
                    itemModel.Changes.Add(uaCItems.results[0]);

                    itemModel.NodeIdItems           = list;

                    var nodeStr                     = nodeId.ToString();
                    if (!nodesList.ContainsKey(nodeStr)) nodesList[nodeStr] = new List<ItemModel>();
                    List<ItemModel> nodes           = nodesList[nodeStr];
                    nodes.Add(itemModel);
                    foreach (var node in nodes)
                    {
                        ClientHandleList[(int)node.ClientHandle] = nodes;
                    }
                    itemModel.ItemModelIndex     = itemModel;
                    uaCItems.results.Add(itemModel);
                    if (App.IsMobile)
                    {
                        uaCItems.SizeHistoricItems = 5;
                        this.listViewMobile.Visibility  = Visibility.Visible;
                        this.listViewDesktop.Visibility = Visibility.Collapsed;
                        this.listViewMobile.DataContext = uaCItems.results;
                    }
                    else
                    {
                        uaCItems.SizeHistoricItems = 10;
                        this.listViewDesktop.Visibility = Visibility.Visible;
                        this.listViewMobile.Visibility  = Visibility.Collapsed;
                        this.listViewDesktop.DataContext = uaCItems.results;
                    }
                }
            }
        }

        private void HistoryReadNext_Click(object sender, RoutedEventArgs e)
        {
            Util.HistoryReadNext_Click(sender, e);
        }

        private void HistoryRelease_Click(object sender, RoutedEventArgs e)
        {
            Util.HistoryRelease_Click(sender, e);
        }

        private async void MonitoredItem_Notification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            if (e.NotificationValue == null)
            {
                return;
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 lock (uaCItems.results)
                 {
                     try
                     {
                         if (e.NotificationValue is MonitoredItemNotification)
                         {
                             MonitoredItemNotification monitoredItemNotification = e.NotificationValue as MonitoredItemNotification;
                             // ServerStatusTB.Text = monitoredItemNotification.Value.ToString();
                            if (ClientHandleList[(int)monitoredItem.ClientHandle] != null)
                            {
                                List<ItemModel> list   = ClientHandleList[(int)monitoredItem.ClientHandle];
                                string value           = String.Empty;
                                List<Grid> arrayItems  = new List<Grid>();
                                if (list.Count > 0)
                                {
                                    if (list[0].AccessLevel.IndexOf("H") != -1 && list[0].BuiltInType != BuiltInType.ExtensionObject)
                                    {
                                        arrayItems.Add(Util.generateGridButton(list[0], HistoryReadNext_Click, 160, "HistoryRead/Next", 160, "HistoryRelease", HistoryRelease_Click));
                                    }
                                }

                                if (monitoredItemNotification.Value != null && monitoredItemNotification.Value.Value != null)
                                {
                                    arrayItems.Add(Util.generateGridNodeId("StatusCode", monitoredItemNotification.Value.StatusCode.ToString()));
                                    arrayItems.Add(Util.generateGridNodeId("Timestamp", monitoredItemNotification.Value.SourceTimestamp.ToString("yyyy-MM-dd hh:mm:ss.fff")));

                                    value = Util.getValue(monitoredItemNotification.Value, arrayItems);

                                    if (list.Count > 0)
                                    {
                                         if (list[0].AccessLevel.IndexOf("W") != -1 && list[0].BuiltInType != BuiltInType.ExtensionObject)
                                         {
                                            arrayItems.Add(Util.generateGridButton(list[0], Util.WriteButton_Click, 100, "Write", 310, value));
                                         }
                                    }

                                    foreach (var node in list)
                                    {
                                         if (node.ClientHandle == monitoredItem.ClientHandle)
                                         {
                                             node.setAttributes(monitoredItemNotification.Value, value);
                                             node.ArrayItems = arrayItems;

                                             LocalizedText lt = monitoredItemNotification.Value.Value as LocalizedText;
                                             if (lt != null)
                                             {
                                                 String str = "\"" + lt.Locale + "\",\"" + lt.Text + "\"";
                                                 node.Value = str;
                                             }
                                             node.AddChange(uaCItems.SizeHistoricItems, node);
                                         }
                                    }
                                }
                                else
                                {
                                    foreach (var node in list)
                                    {
                                         if (node.ClientHandle == monitoredItem.ClientHandle)
                                         {
                                             node.SolidColorBrush = ItemModel.solidColorBrushBlack;
                                             if (monitoredItemNotification.Value != null)
                                             {
                                                 node.setAttributes(monitoredItemNotification.Value, monitoredItemNotification.Value.ToString());
                                             }
                                             else
                                             {
                                                 node.StatusCodeStr = String.Empty;
                                                 node.Timestamp = String.Empty;
                                                 node.Value = String.Empty;
                                             }
                                             node.AddChange(uaCItems.SizeHistoricItems, node);
                                         }
                                    }
                                }
                            }
                        }
                        else
                        {
                            XmlEncoder encoder = new XmlEncoder(monitoredItem.Subscription.Session.MessageContext);
                            e.NotificationValue.Encode(encoder);
                            ServerStatusTB.Text = encoder.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.Trace(ex, "Error processing monitored item notification.");
                    }
                }
            });
        }

        private void Task_TestMI_Click(object sender, EventArgs e)
        {
            try
            {
                DoTest(uaCItems.m_session);
            }
            catch (Exception exception)
            {
                GuiUtils.HandleException(String.Empty, GuiUtils.CallerName(), exception);
            }
        }

        private void ComboBoxNodeIdDesktop_DropDownOpened(object sender, object e)
        {
            Util.ComboBoxNodeId_DropDownOpened(sender, e);
        }

        private void ComboBoxNodeIdMobile_DropDownOpened(object sender, object e)
        {
            Util.ComboBoxNodeId_DropDownOpened(sender, e);
        }

        private void ComboBoxNodeIdDesktop_DropDownClosed(object sender, object e)
        {
            Util.ComboBoxNodeId_DropDownClosed(sender, e);
        }

        private void ComboBoxNodeIdMobile_DropDownClosed(object sender, object e)
        {
            Util.ComboBoxNodeId_DropDownClosed(sender, e);
        }
    }
}
