using EGamePlay;

public class WorkFlowSource : Entity
{
    private WorkFlow CurrentWorkFlow { get; set; }
    public WorkFlow PostWorkFlow { get; private set; }
    public WorkFlow ToEnter<T>() where T : WorkFlow
    {
        var workflow = AddChild<T>();
        PostWorkFlow = workflow;
        workflow.FlowSource = this;
        return workflow;
    }
    public void Startup()
    {
        CurrentWorkFlow = PostWorkFlow;
        CurrentWorkFlow.Startup();
    }
    public void OnFlowFinish()
    {
        CurrentWorkFlow = CurrentWorkFlow.PostWorkFlow;
        CurrentWorkFlow.Startup();
    }
}
