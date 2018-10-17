﻿using GiantCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace GiantNode
{
    /// <summary>
    /// 节点句柄
    /// </summary>
    public class NodeHandle
    {
        public NodeHandle(string dllName, NodeRuntime runtime)
        {
            Assembly assembly = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + dllName);

            Type[] typeList = assembly.GetTypes();

            List<Type> entryList = new List<Type>();

            foreach (var curr in typeList)
            {
                if (curr.GetInterface("IPlugin") != null)
                {
                    entryList.Add(curr);
                }
            }

            if (entryList.Count > 1)
            {
                throw new Exception(string.Format("Plugin {0} Can't Have More Then One Class Inherited interface IPlugin ", dllName));
            }

            PluginEntryAttribute pluginAttribute = entryList[0].GetCustomAttribute(typeof(PluginEntryAttribute)) as PluginEntryAttribute;
            if (pluginAttribute == null)
            {
                throw new Exception(string.Format("Plugin {0} Have Not PluginEntryAttribute", dllName));
            }

            IPlugin plugin = Activator.CreateInstance(entryList[0]) as IPlugin;
            if (plugin == null)
            {
                throw new Exception(string.Format("Plugin {0} CreateInstance Error !", dllName));
            }

            mRunTime = runtime;

            mNodeEvent = plugin.Events;

            mRunTime.IsFrontNode = pluginAttribute.IsFrontNode;

            mNodeName = pluginAttribute.PluginName;
        }

        /// <summary>
        /// 开始启动
        /// </summary>
        public void ToStart()
        {
            //初始化插件事件
            mNodeEvent.OnNodeInit(mRunTime.Param);

            ThreadHelper.CreateThreadInThreadPool(UpdateLoop, null);

            //内部通讯服务
            InnerNetServer.Init(mRunTime);

            if (IsFrontNode)
            {
                int port = 0;

                if (!int.TryParse(mRunTime.GetParam("FrontPort"), out port))
                {
                    throw new ArgumentNullException(string.Format("Node {0}_{1} FrontPort can't be null ", mRunTime.NodeName, mRunTime.NodeId));
                }

                OutNetServer.Init(port);
            }

            //插件启动完成事件
            mNodeEvent.OnNodeStartComplate();
        }

        /// <summary>
        /// 工作循环
        /// </summary>
        private void UpdateLoop(object param)
        {
            while (true)
            {
                DateTime now = DateTime.Now;

                float delay = (float)(now - mLastUpdateTime).TotalMilliseconds;

                mLastUpdateTime = now;

                if (delay > 10)
                {
                    Log.LogOut(LogType.Warning, string.Format("节点{0} {1}s 没有响应了!", mNodeName, delay));
                }

                //插件跟新事件
                mNodeEvent.OnNodeUpdate(delay);

                Thread.Sleep(10);
            }
        }


        public bool IsFrontNode
        {
            get { return mRunTime.IsFrontNode; }
        }

        private NodeRuntime mRunTime = null;

        private DateTime mLastUpdateTime = DateTime.Now;

        private readonly string mNodeName = "";

        private readonly NodeEvents mNodeEvent = null;
    }
}