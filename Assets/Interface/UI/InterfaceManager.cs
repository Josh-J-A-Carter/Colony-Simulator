using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class InterfaceManager : MonoBehaviour {

    public static InterfaceManager Instance { get; private set; }


    VisualElement containerRoot;
    // Config containers
    VisualElement configurableContainerRoot, configurableContainerContentRoot;
    VisualElement configInfoContainerRoot, configInfoContainerContentRoot;

    // Info containers
    VisualElement infoContainerRoot, infoContainerContentRoot;
    VisualElement taskInfoContainerRoot, taskInfoContainerContentRoot;


    VisualElement forageContainerRoot, forageNewContainerRoot, forageOldContainerRoot, forageWarnLabel;
    Button forageMenuQuit;

    List<Button> toolButtons;

    InputManager tm => InputManager.Instance;

    public void Awake() {
        // Instantiate singleton
        if (Instance != null) {
            Destroy(this);
            return;
        } else Instance = this;

        containerRoot = GetComponent<UIDocument>().rootVisualElement;

        // Register tool button callbacks
        toolButtons = containerRoot.Query<Button>().ToList();

        Button select = containerRoot.Q(name: "select-tool") as Button;
        select.AddToClassList("selected");

        select.RegisterCallback<ClickEvent>(ClickedSelectTool);
        containerRoot.Q(name: "construct-tool").RegisterCallback<ClickEvent>(ClickedConstructTool);
        containerRoot.Q(name: "destroy-tool").RegisterCallback<ClickEvent>(ClickedDestroyTool);
        containerRoot.Q(name: "forage-tool").RegisterCallback<ClickEvent>(ClickedForageTool);
        containerRoot.Q(name: "cancel-tool").RegisterCallback<ClickEvent>(ClickedCancelTool);

        // Configurable container
        configurableContainerRoot = containerRoot.Q(name: "configurable-container-root");
        configurableContainerContentRoot = containerRoot.Q(name: "configurable-container-content-root");
        // Config info (goes beside config container when a selection is made)
        configInfoContainerRoot = containerRoot.Q(name: "config-info-container-root");
        configInfoContainerContentRoot = containerRoot.Q(name: "config-info-container-content-root");

        // Info container container
        infoContainerRoot = containerRoot.Q(name: "info-container-root");
        infoContainerContentRoot = containerRoot.Q(name: "info-container-content-root");
        // Task info container
        taskInfoContainerRoot = containerRoot.Q(name: "task-info-container-root");
        taskInfoContainerContentRoot = containerRoot.Q(name: "task-info-container-content-root");

        // Forage menu
        forageContainerRoot = containerRoot.Q(name: "forage-menu");
        forageNewContainerRoot = containerRoot.Q(name: "new-rule-contents");
        forageOldContainerRoot = containerRoot.Q(name: "old-rule-contents");
        forageMenuQuit = containerRoot.Q(name: "forage-menu-quit") as Button;
        forageWarnLabel = forageContainerRoot.Q(name: "warning-label");


        MakeTogglesUnfocusable();
    }

    public void Start() {
        List<(TaskPriority, String)> options = Enum.GetValues(typeof(TaskPriority))
                                                .OfType<TaskPriority>()
                                                .ToList()
                                                .Select(t => (t, Enum.GetName(typeof(TaskPriority), t)))
                                                .Reverse()
                                                .ToList();

        OptionDisplay<TaskPriority> display = new(
            options.Count - 1 - (int) InputManager.Instance.GetPriority(),
            options,
            priority => InputManager.Instance.SetPriority(priority),
            ClickedPriorityTool
        );

        display.name = "priority-tool";

        containerRoot.Q(name: "toolbar").Add(display);
        toolButtons.Add(display);
    }


    void MakeTogglesUnfocusable() {
        foreach (Toggle toggle in containerRoot.Query<Toggle>().Build()) {
            toggle.focusable = false;
        }
    }

    public void SetConfigurableContainerContent(VisualElement content) {
        configurableContainerContentRoot.Clear();

        configurableContainerContentRoot.Add(content);
    }

    public void ShowConfigurableContainer() {
        configurableContainerRoot.style.visibility = Visibility.Visible;
    }

    public void HideConfigurableContainer() {
        configurableContainerRoot.style.visibility = Visibility.Hidden;
    }


    public void ShowConfigInfoContainer() {
        configInfoContainerRoot.style.visibility = Visibility.Visible;
    }

    public void HideConfigInfoContainer() {
        configInfoContainerRoot.style.visibility = Visibility.Hidden;
    }

    public void SetConfigInfoContainerContent(VisualElement content) {
        configInfoContainerContentRoot.Clear();

        configInfoContainerContentRoot.Add(content);
    }

    public void SetInfoContainerContent(VisualElement content) {
        infoContainerContentRoot.Clear();

        infoContainerContentRoot.Add(content);
    }

    public void SetTaskInfoContainerContent(VisualElement content) {
        taskInfoContainerContentRoot.Clear();

        taskInfoContainerContentRoot.Add(content);
    }

    public void ShowInfoContainer() {
        infoContainerRoot.style.display = DisplayStyle.Flex;
    }

    public void HideInfoContainer() {
        infoContainerRoot.style.display = DisplayStyle.None;
    }

    public void ShowTaskInfoContainer() {
        taskInfoContainerRoot.style.display = DisplayStyle.Flex;
    }

    public void HideTaskInfoContainer() {
        taskInfoContainerRoot.style.display = DisplayStyle.None;
    }

    void DeselectAllButtons() {
        foreach (Button b in toolButtons) b.RemoveFromClassList("selected");
    }

    public void ClickedSelectTool(ClickEvent _) {
        tm.SetTool(ToolType.Select);

        DeselectAllButtons();

        containerRoot.Q(name: "select-tool").AddToClassList("selected");
    }

    void ClickedConstructTool(ClickEvent _) {
        tm.SetTool(ToolType.Build);

        DeselectAllButtons();

        containerRoot.Q(name: "construct-tool").AddToClassList("selected");
    }

    void ClickedDestroyTool(ClickEvent _) {
        tm.SetTool(ToolType.Destroy);

        DeselectAllButtons();

        containerRoot.Q(name: "destroy-tool").AddToClassList("selected");
    }

    void ClickedForageTool(ClickEvent _) {
        tm.SetTool(ToolType.Forage);
    }

    void ClickedCancelTool(ClickEvent _) {
        tm.SetTool(ToolType.Cancel);

        DeselectAllButtons();

        containerRoot.Q(name: "cancel-tool").AddToClassList("selected");
    }

    void ClickedPriorityTool() {
        DeselectAllButtons();

        tm.SetTool(ToolType.Priority);

        containerRoot.Q(name: "priority-tool").AddToClassList("selected");
    }

    public void ShowForageMenu() {
        forageContainerRoot.style.visibility = Visibility.Visible;
    }

    public void HideForageMenu() {
        forageContainerRoot.style.visibility = Visibility.Hidden;
    }

    public void SetForageQuitCallback(EventCallback<ClickEvent> callback) {
        forageMenuQuit.RegisterCallback(callback);
    }

    public void AddOldForageContent<T, Q>(RuleDisplay<T, Q> display) {
        forageOldContainerRoot.Add(display);
    }

    public void ResetOldForageContent() {
        forageOldContainerRoot.Clear();
    }

    public void SetNewForageContent<T, Q>(RuleDisplay<T, Q> display) {
        forageNewContainerRoot.Clear();
        forageNewContainerRoot.Add(display);
    }

    public void ShowForageWarning() {
        forageWarnLabel.visible = true;
    }

    public void HideForageWarning() {
        forageWarnLabel.visible = false;
    }
}
