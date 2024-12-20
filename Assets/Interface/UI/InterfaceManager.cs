using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InterfaceManager : MonoBehaviour {

    public static InterfaceManager Instance { get; private set; }


    VisualElement containerRoot;
    VisualElement configurableContainerRoot, configurableContainerContentRoot;

    VisualElement configInfoContainerRoot, configInfoContainerContentRoot;

    VisualElement infoContainerRoot, infoContainerContentRoot;


    VisualElement forageContainerRoot, forageNewContainerRoot, forageOldContainerRoot;
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

        // Configurable container
        configurableContainerRoot = containerRoot.Q(name: "configurable-container-root");
        configurableContainerContentRoot = containerRoot.Q(name: "configurable-container-content-root");
        // Config info (goes beside config container when a selection is made)
        configInfoContainerRoot = containerRoot.Q(name: "config-info-container-root");
        configInfoContainerContentRoot = containerRoot.Q(name: "config-info-container-content-root");

        // Configurable container
        infoContainerRoot = containerRoot.Q(name: "info-container-root");
        infoContainerContentRoot = containerRoot.Q(name: "info-container-content-root");

        // Forage menu
        forageContainerRoot = containerRoot.Q(name: "forage-menu");
        forageNewContainerRoot = containerRoot.Q(name: "new-rule-contents");
        forageOldContainerRoot = containerRoot.Q(name: "old-rule-contents");
        forageMenuQuit = containerRoot.Q(name: "forage-menu-quit") as Button;


        MakeTogglesUnfocusable();
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

    public void ShowInfoContainer() {
        infoContainerRoot.style.visibility = Visibility.Visible;
    }

    public void HideInfoContainer() {
        infoContainerRoot.style.visibility = Visibility.Hidden;
    }

    void DeselectAllButtons() {
        foreach (Button b in toolButtons) b.RemoveFromClassList("selected");
    }

    public void ClickedSelectTool(ClickEvent evt) {
        tm.SetTool(ToolType.Select);

        DeselectAllButtons();

        containerRoot.Q(name: "select-tool").AddToClassList("selected");
    }

    void ClickedConstructTool(ClickEvent evt) {
        tm.SetTool(ToolType.Build);

        DeselectAllButtons();

        containerRoot.Q(name: "construct-tool").AddToClassList("selected");
    }

    void ClickedDestroyTool(ClickEvent evt) {
        tm.SetTool(ToolType.Destroy);

        DeselectAllButtons();

        containerRoot.Q(name: "destroy-tool").AddToClassList("selected");
    }

    void ClickedForageTool(ClickEvent evt) {
        tm.SetTool(ToolType.Forage);
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

    public void AddOldForageContent(RuleDisplay display) {
        forageOldContainerRoot.Add(display);
    }

    public void ResetOldForageContent() {
        forageOldContainerRoot.Clear();
    }

    public void SetNewForageContent(RuleDisplay display) {
        forageNewContainerRoot.Clear();
        forageNewContainerRoot.Add(display);
    }
}
