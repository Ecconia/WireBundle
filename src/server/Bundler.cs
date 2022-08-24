using LogicAPI.Server.Components;

using WireBundle.Server;

namespace WireBundle.Components
{
    public class Bundler : LogicComponent
    {
        protected override void Initialize()
        {
            Bundlers.Components.Add(Outputs[0].Address, this);
        }

        public override void OnComponentDestroyed()
        {
            Bundlers.Components.Remove(Outputs[0].Address);
        }

        protected override void DoLogicUpdate()
        {
            bool active = false;
            foreach(var input in Inputs)
            {
                if(input.On)
                {
                    active = true;
                    break;
                }
            }
            Outputs[0].On = active;
        }
    }
}