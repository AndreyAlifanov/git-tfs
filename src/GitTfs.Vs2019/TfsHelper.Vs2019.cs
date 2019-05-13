using System;
using GitTfs.VsCommon;
using StructureMap;
using Microsoft.VisualStudio.Setup.Configuration;
using System.IO;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Client;

namespace GitTfs.Vs2019
{
    public class TfsHelper : TfsHelperVs2012Base
    {
        protected override string TfsVersionString { get { return "16.0"; } }

        public TfsHelper(TfsApiBridge bridge, IContainer container)
            : base(bridge, container)
        { }

        protected override string GetDialogAssemblyPath()
        {
#if NETFRAMEWORK
            var tfsExtensionsFolder = Path.Combine(GetVsInstallDir(), @"Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer");
            return Path.Combine(tfsExtensionsFolder, DialogAssemblyName + ".dll");
#else
            Trace.TraceWarning("Checkin dialog is not supported with dotnet core version of git-tfs");
            return string.Empty;
#endif
        }

        private string vsInstallDir;
        protected override string GetVsInstallDir()
        {
            if (vsInstallDir != null)
                return vsInstallDir;

            var query = new SetupConfiguration();
            var query2 = (ISetupConfiguration2)query;
            var e = query2.EnumAllInstances();

            int fetched;
            var instances = new ISetupInstance[1];
            do
            {
                e.Next(1, instances, out fetched);
                if (fetched > 0)
                {
                    vsInstallDir = instances[0].GetInstallationPath();
                    break;
                }
            }
            while (fetched > 0);
            return vsInstallDir;
        }
        protected override TfsTeamProjectCollection GetTfsCredential(Uri uri)
        {
            var winCred = HasCredentials ?
                new Microsoft.VisualStudio.Services.Common.WindowsCredential(GetCredential()) :
                new Microsoft.VisualStudio.Services.Common.WindowsCredential(true);
            var vssCred = new VssClientCredentials(winCred);

            return new TfsTeamProjectCollection(uri, vssCred);
        }
    }
}
