using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class PriorityQueue<T> where T : class {

    struct Node<S> {
        public S data;
        public int priority;
    }
    
    List<Node<T>> queue;

    public PriorityQueue() {
        queue = new List<Node<T>>();
    }

    public void Add(T data, int priority) {

    }

    public void Remove(T data) {
        
    }

    public T peek() {
        return null;
    }

}
