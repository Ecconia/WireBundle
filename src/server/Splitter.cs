using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using LogicAPI.Server.Components;

using LogicWorld.Server.Circuitry;

using WireBundle.Server;

namespace WireBundle.Components
{
    public class Splitter : LogicComponent
    {
        private List<Bundler> connectedBundlers = new List<Bundler>();

        private static readonly PropertyInfo clusterProperty = typeof(InputPeg).GetProperty("Cluster", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo linkerField = typeof(Cluster).GetField("Linker", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo linkedLeaders = typeof(ClusterLinker).GetField("LinkedLeaders", BindingFlags.NonPublic | BindingFlags.Instance);

        private List<Bundler> getAllConnectedBundlers()
        {
            List<Bundler> foundBundlers = new List<Bundler>();
            Cluster cluster = (Cluster)clusterProperty.GetValue(Inputs[0]);
            ClusterLinker linker = (ClusterLinker)linkerField.GetValue(cluster);
            foreach (OutputPeg pegTC in cluster.ConnectedOutputs)
            {
                if (Bundlers.Components.TryGetValue(pegTC.Address, out Bundler connectedComponent))
                {
                    foundBundlers.Add(connectedComponent);
                }
            }
            if (linker != null)
            {
                foreach (ClusterLinker connectedClusterLinker in (List<ClusterLinker>)linkedLeaders.GetValue(linker))
                {
                    foreach (OutputPeg pegTC in connectedClusterLinker.ClusterBeingLinked.ConnectedOutputs)
                    {
                        if (Bundlers.Components.TryGetValue(pegTC.Address, out Bundler connectedComponent))
                        {
                            foundBundlers.Add(connectedComponent);
                        }
                    }
                }
            }
            return foundBundlers;
        }
        
        protected override void DoLogicUpdate()
        {
            List<Bundler> foundBundlersToTest = getAllConnectedBundlers();

            HashSet<Bundler> removedBundlers = new HashSet<Bundler>(connectedBundlers);
            List<Bundler> newBundlers = new List<Bundler>();
            foreach(Bundler bundler in foundBundlersToTest)
            {
                if(!removedBundlers.Remove(bundler))
                {
                    newBundlers.Add(bundler);
                }
            }
            if(removedBundlers.Count != 0)
            {
                foreach (Bundler toRemove in removedBundlers)
                {
                    int maxLocal = base.Inputs.Count - 1; //Splitter
                    int maxRemote = toRemove.Inputs.Count; //Bundler
                    int maxLinks = maxRemote < maxLocal ? maxRemote : maxLocal;
                    maxRemote -= 1;
                    for (int i = 0; i < maxLinks; i++)
                    {
                        toRemove.Inputs[maxRemote - i].RemoveOneWayPhasicLinkTo(base.Inputs[maxLocal - i]);
                    }
                }
            }
            if(newBundlers.Count != 0)
            {
                foreach (Bundler toAdd in newBundlers)
                {
                    int maxLocal = base.Inputs.Count - 1; //Splitter
                    int maxRemote = toAdd.Inputs.Count; //Bundler
                    int maxLinks = maxRemote < maxLocal ? maxRemote : maxLocal;
                    maxRemote -= 1;
                    for (int i = 0; i < maxLinks; i++)
                    {
                        toAdd.Inputs[maxRemote - i].AddOneWayPhasicLinkTo(base.Inputs[maxLocal - i]);
                    }
                }
            }
            connectedBundlers = foundBundlersToTest;
            
            QueueLogicUpdate();
        }
    }
}