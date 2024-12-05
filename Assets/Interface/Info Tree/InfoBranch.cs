using System;
using System.Collections.Generic;

public class InfoBranch : InfoNode {

    List<InfoNode> children;

    public InfoBranch(String categoryName) {
        this.categoryName = categoryName;
        children = new List<InfoNode>();
    }

    public List<InfoNode> GetChildren() {
        return children;
    }

    public void AddChild(InfoNode node) {
        children.Add(node);
    }

}
