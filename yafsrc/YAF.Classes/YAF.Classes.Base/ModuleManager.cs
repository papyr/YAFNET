﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web.UI;
using YAF.Classes.Base;
using YAF.Classes.Utils;

namespace YAF.Modules
{
	public class ModuleManager
	{
		private List<IBaseModule> _loadedModules = null;
		private List<Type> _moduleClassTypes = null;

		public ModuleManager()
		{
			_moduleClassTypes = new List<Type>();
			_loadedModules = new List<IBaseModule>();

			if (YafCache.Current["YafModuleManager_Modules"] == null)
			{
				// get classes...
				foreach ( Assembly assembly in System.Web.Compilation.BuildManager.CodeAssemblies )
				{
					TypeFilter filter = new TypeFilter( BaseModuleFilter );

					foreach ( Module module in assembly.GetModules() )
					{
						foreach ( Type modClass in module.FindTypes( filter, "YAF.Modules" ) )
						{
							// YAF.Modules namespace, verify it implements the interface...
							Type[] interfaces = modClass.GetInterfaces();

							foreach ( Type inter in interfaces )
							{
								if ( inter.ToString() == "YAF.Modules.IBaseModule" )
									_moduleClassTypes.Add( modClass );
							}
						}
					}
				}

				YafCache.Current["YafModuleManager_Modules"] = _moduleClassTypes;
			}
			else
			{
				_moduleClassTypes = YafCache.Current["YafModuleManager_Modules"] as List<Type>;
			}
		}

		public void CreateModules()
		{
			foreach (Type module in _moduleClassTypes)
			{
				IBaseModule customModule = (IBaseModule)Activator.CreateInstance(module);
				_loadedModules.Add(customModule);
			}			
		}

		public void InitModulesBeforeForumPage(YafContext currentContext, object forumControl, ForumPages pageType)
		{
			foreach (IBaseModule currentModule in _loadedModules)
			{
				if (currentModule.InitBeforeForumPage)
				{
					currentModule.Initalize(currentContext, forumControl, null, pageType);
				}
			}
		}

		public void InitModulesAfterForumPage(YafContext currentContext, object forumControl, ForumPage forumPage, ForumPages pageType)
		{
			foreach (IBaseModule currentModule in _loadedModules)
			{
				if (!currentModule.InitBeforeForumPage)
				{
					currentModule.Initalize(currentContext, forumControl, forumPage, pageType);
				}
				else
				{
					currentModule.ForumPage = forumPage;
				}
			}
		}

		public static bool BaseModuleFilter(Type typeObj, Object criteriaObj)
		{
			if ( typeObj.Namespace == criteriaObj.ToString())
				return true;
			else
				return false;
		}
	}
}
