using System;

namespace RouletteDeluxe
{
    // CFeatureMgr::PlayFeatPvt / PlaySeqFeature in the original RCGTopbox.
    // Graphics and timers remain in the supplied host, including cancellation.
    public sealed class OriginalFeatureGraphPlayback
    {
        public sealed class Host
        {
            public Action<OriginalFeatureGraph.Atom, bool, long, short, short, Action> PlayObject;
            public Action<OriginalFeatureGraph.Atom> StopObject;
            public Action<int,OriginalFeatureTransition,Action<short,short>,Action> StartTransition;
            public Action<int> StopTransition;
            public Action<OriginalFeatureGraph.Atom,short,short> MoveObject;
            public Action<int,int> Notify;
        }
        private sealed class Live
        {
            public bool Active,AllObjectsDone;
            public int Cursor,PendingChildren,MoneyCursor,Notify;
            public long[] Money;
            public Action AttachedCompletion;
        }
        private readonly OriginalFeatureGraph graph;
        private readonly Host host;
        private readonly Live[] live;
        public OriginalFeatureGraphPlayback(OriginalFeatureGraph graph,Host host)
        {
            if(graph==null||host==null||host.PlayObject==null||host.StopObject==null||host.StartTransition==null||
                host.StopTransition==null||host.MoveObject==null||host.Notify==null)
                throw new ArgumentException("Complete original Feature media owner required");
            this.graph=graph;this.host=host;live=new Live[graph.Count];
            for(int i=0;i<live.Length;i++)live[i]=new Live();
        }
        public bool IsActive(int id) {return live[id].Active;}
        public void Play(int id,long[] money,int notify=-1)
        {
            if(id<0||id>=graph.Count)throw new ArgumentOutOfRangeException("id");
            Live current=live[id];OriginalFeatureGraph.Feature definition=graph[id];
            current.Active=true;current.AllObjectsDone=false;current.Cursor=-1;
            current.PendingChildren=0;current.MoneyCursor=0;current.Notify=notify;
            current.Money=money==null?new long[0]:(long[])money.Clone();
            if(definition.TransitionMilliseconds>0)
            {
                host.StopTransition(id);
                var transition=new OriginalFeatureTransition(definition.StartX,definition.StartY,
                    definition.EndX,definition.EndY,definition.TransitionMilliseconds,definition.Sequential);
                transition.IsOwnerActive=delegate{return live[id].Active;};
                host.StartTransition(id,transition,delegate(short x,short y)
                {
                    if(!live[id].Active)return;
                    foreach(OriginalFeatureGraph.Atom atom in graph[id].Objects)
                        if(atom.Type!="Audio")host.MoveObject(atom,x,y);
                },delegate {if(live[id].Notify>=0)host.Notify(live[id].Notify,id);});
            }
            if(definition.Sequential){Advance(id,false);return;}
            foreach(OriginalFeatureGraph.Atom atom in definition.Objects)PlayObject(id,atom,null);
            foreach(OriginalFeatureGraph.Trigger trigger in definition.Triggers)Trigger(id,trigger,false);
            if(current.Notify>=0&&definition.TransitionMilliseconds==0)host.Notify(current.Notify,id);
        }
        public void Stop(int id)
        {
            if(id==-1){for(int i=0;i<live.Length;i++)Stop(i);return;}
            if(id<0||id>=live.Length)throw new ArgumentOutOfRangeException("id");
            Live current=live[id];current.Active=false;current.Notify=-1;current.Cursor=0;
            current.AllObjectsDone=false;current.PendingChildren=0;current.MoneyCursor=0;
            foreach(OriginalFeatureGraph.Atom atom in graph[id].Objects)host.StopObject(atom);
        }
        private void PlayObject(int id,OriginalFeatureGraph.Atom atom,Action completed)
        {
            Live current=live[id];bool hasMoney=atom.Type=="Meter";long value=0;
            if(hasMoney)
            {
                if(current.MoneyCursor>=current.Money.Length)
                    throw new InvalidOperationException("Original Feature meter has no supplied value: "+graph[id].Name);
                value=current.Money[current.MoneyCursor++];
            }
            OriginalFeatureGraph.Feature definition=graph[id];
            host.PlayObject(atom,hasMoney,value,definition.StartX,definition.StartY,completed);
        }
        private void Trigger(int id,OriginalFeatureGraph.Trigger trigger,bool attach)
        {
            Live current=live[id];
            if(trigger.Name.Length!=0)
            {
                int child=graph.Id(trigger.Name);
                long[] money=new long[0];
                if(trigger.WinDataOffset>=0&&trigger.WinDataOffset<current.Money.Length)
                {
                    money=new long[current.Money.Length-trigger.WinDataOffset];
                    Array.Copy(current.Money,trigger.WinDataOffset,money,0,money.Length);
                }
                if(attach&&trigger.WaitForCompletion)
                {
                    current.PendingChildren=unchecked(current.PendingChildren+1);
                    live[child].AttachedCompletion=delegate{Advance(id,true);};
                }
                Play(child,money);
            }
            if(trigger.KillName.Length!=0)Stop(graph.Id(trigger.KillName));
        }
        private void Advance(int id,bool childCompleted)
        {
            Live current=live[id];OriginalFeatureGraph.Feature definition=graph[id];
            if(!current.Active)return;
            if(childCompleted)current.PendingChildren=unchecked(current.PendingChildren-1);
            else
            {
                current.Cursor=unchecked(current.Cursor+1);
                // Native only scans triggers while cursor <= trigger count.
                if(current.Cursor<=definition.Triggers.Count&&definition.Triggers.Count>0)
                    foreach(OriginalFeatureGraph.Trigger trigger in definition.Triggers)
                        if(trigger.Index==current.Cursor)Trigger(id,trigger,true);
                if(current.Cursor>=definition.Objects.Count)current.AllObjectsDone=true;
                else PlayObject(id,definition.Objects[current.Cursor],delegate{Advance(id,false);});
            }
            if(!current.AllObjectsDone||current.PendingChildren!=0)return;
            current.Active=false;
            if(current.Notify>=0)host.Notify(current.Notify,id);
            else if(current.AttachedCompletion!=null)
            {
                current.AttachedCompletion();
                current.AttachedCompletion=null;
            }
        }
    }
}
