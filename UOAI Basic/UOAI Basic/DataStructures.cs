using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{

    //AVL Balanced Binary Tree implementation
    //- ~o(log(n)) average/worstcase performance
    //- size ~ n
    //So a well-sized datastructure with an acceptable peformance.

    public class BinaryTreeEnumerator<TValue> : IEnumerator<TValue>
    {
        private List<TValue> m_SnapShot;
        private int m_CurIdx;
        private int m_Count;

        #region IEnumerator<TValue> Members

        public BinaryTreeEnumerator(List<TValue> snapshot)
        {
            m_CurIdx = -1;
            m_Count = snapshot.Count;
            m_SnapShot = snapshot;
        }

        public TValue Current
        {
            get
            {
                if (m_CurIdx == -1)
                    m_CurIdx = 0;
                if (m_CurIdx < m_Count)
                    return m_SnapShot[m_CurIdx];
                else
                    return default(TValue);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            return;
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get
            {
                if (m_CurIdx == -1)
                    m_CurIdx = 0;
                if (m_CurIdx < m_Count)
                    return m_SnapShot[m_CurIdx];
                else
                    return default(TValue);
            }
        }

        public bool MoveNext()
        {
            m_CurIdx++;
            if (m_CurIdx < m_Count)
                return true;
            else
                return false;
        }

        public void Reset()
        {
            m_CurIdx = -1;
        }

        #endregion
    }

    internal class BinaryTreeNode<TKey, TValue>
    {
        public TKey key;
        public TValue value;
        public BinaryTreeNode<TKey, TValue> parent;
        public BinaryTreeNode<TKey, TValue> leftchild;
        public BinaryTreeNode<TKey, TValue> rightchild;
        public int height;

        public BinaryTreeNode(TKey btnkey, TValue btnvalue, BinaryTreeNode<TKey, TValue> btnparent)
        {
            key = btnkey;
            value = btnvalue;
            parent = btnparent;
            leftchild = null;
            rightchild = null;
            height = 1;
        }
    }

    public class BinaryTree<TKey, TValue> : IEnumerable<TValue>, IDictionary<TKey, TValue>
    {
        internal uint m_Count;
        internal BinaryTreeNode<TKey, TValue> root;

        public uint Count { get { return m_Count; } }

        /*public TValue this[int index]
        {
            get
            {
               lock(this)
               {
                    if (index < m_Count)
                    {
                        BinaryTreeNode<TKey, TValue> curnode = leftmost();
                        for (uint i = 0; i < index; i++)
                            curnode = InOrderNext(curnode);
                        return curnode.value;
                    }
                }
                return default(TValue);
            }
        }*/

        public BinaryTree()
        {
            m_Count = 0;
            root = null;
        }

        private void BalanceUp(BinaryTreeNode<TKey, TValue> fromnode)
        {
            int curbalance;

            while (true)
            {
                curbalance = GetBalance(fromnode);
                if (curbalance < -1)
                {
                    if (GetBalance(fromnode.leftchild) > 0)
                        fromnode.leftchild = RotateLeft(fromnode.leftchild);
                    fromnode = RotateRight(fromnode);
                }
                else if (curbalance > 1)
                {
                    if (GetBalance(fromnode.rightchild) < 0)
                        fromnode.rightchild = RotateRight(fromnode.rightchild);
                    fromnode = RotateLeft(fromnode);
                }
                RecalculateHeight(fromnode);
                if (fromnode.parent == null)
                {
                    root = fromnode;
                    break;
                }
                else
                    fromnode = fromnode.parent;
            }

            return;
        }

        private void RecalculateHeight(BinaryTreeNode<TKey, TValue> node)
        {
            if ((node.leftchild != null) && (node.rightchild != null))
            {
                node.height = Math.Max(node.leftchild.height, node.rightchild.height) + 1;
            }
            else if (node.leftchild != null)
            {
                node.height = node.leftchild.height + 1;
            }
            else if (node.rightchild != null)
            {
                node.height = node.rightchild.height + 1;
            }
            else
            {
                node.height = 1;
            }
        }

        private BinaryTreeNode<TKey, TValue> RotateLeft(BinaryTreeNode<TKey, TValue> torotate)
        {
            BinaryTreeNode<TKey, TValue> oldparent = torotate.parent;
            BinaryTreeNode<TKey, TValue> newtop = torotate.rightchild;

            torotate.rightchild = newtop.leftchild;
            if (newtop.leftchild != null)
                newtop.leftchild.parent = torotate;

            newtop.leftchild = torotate;
            torotate.parent = newtop;

            newtop.parent = oldparent;
            if (oldparent != null)
            {
                if (oldparent.leftchild == torotate)
                {
                    oldparent.leftchild = newtop;
                }
                else//if(oldparent.rightchild==torotate)
                {
                    oldparent.rightchild = newtop;
                }
            }

            RecalculateHeight(torotate);
            RecalculateHeight(newtop);

            return newtop;
        }

        private BinaryTreeNode<TKey, TValue> RotateRight(BinaryTreeNode<TKey, TValue> torotate)
        {
            BinaryTreeNode<TKey, TValue> oldparent = torotate.parent;
            BinaryTreeNode<TKey, TValue> newtop = torotate.leftchild;

            torotate.leftchild = newtop.rightchild;
            if (newtop.rightchild != null)
                newtop.rightchild.parent = torotate;

            newtop.rightchild = torotate;
            torotate.parent = newtop;

            newtop.parent = oldparent;
            if (oldparent != null)
            {
                if (oldparent.leftchild == torotate)
                {
                    oldparent.leftchild = newtop;
                }
                else//if(oldparent.rightchild==torotate)
                {
                    oldparent.rightchild = newtop;
                }
            }

            RecalculateHeight(torotate);
            RecalculateHeight(newtop);

            return newtop;
        }

        private int GetBalance(BinaryTreeNode<TKey, TValue> ofnode)
        {
            if ((ofnode.leftchild != null) && (ofnode.rightchild != null))
                return ofnode.rightchild.height - ofnode.leftchild.height;
            else if (ofnode.leftchild != null)
                return 0 - ofnode.leftchild.height;
            else if (ofnode.rightchild != null)
                return ofnode.rightchild.height - 0;
            else
                return 0;
        }

        internal BinaryTreeNode<TKey, TValue> FindInsertionPoint(TKey tofind, out int lastcomparisonresult)
        {
            lock (this)
            {
                BinaryTreeNode<TKey, TValue> curnode;
                lastcomparisonresult = 0;
                curnode = root;
                while (curnode != null)
                {
                    lastcomparisonresult = Comparer<TKey>.Default.Compare(curnode.key, tofind);
                    if ((lastcomparisonresult > 0) && (curnode.leftchild != null))
                        curnode = curnode.leftchild;
                    else if ((lastcomparisonresult < 0) && (curnode.rightchild != null))
                        curnode = curnode.rightchild;
                    else
                        return curnode;
                }
                return curnode;
            }
        }

        public bool Insert(TKey key, TValue val)
        {
            lock(this)
            {
                if (root == null)
                {
                    root = new BinaryTreeNode<TKey, TValue>(key, val, null);
                    m_Count++;
                    return true;
                }

                int compres;
                BinaryTreeNode<TKey, TValue> insertionpoint = FindInsertionPoint(key, out compres);
                if (compres > 0)
                    insertionpoint.leftchild = new BinaryTreeNode<TKey, TValue>(key, val, insertionpoint);
                else if (compres < 0)
                    insertionpoint.rightchild = new BinaryTreeNode<TKey, TValue>(key, val, insertionpoint);
                else
                {
                    return false;
                }

                m_Count++;

                BalanceUp(insertionpoint);

                return true;
            }
        }

        public bool Find(TKey tofind, out TValue found)
        {
            lock(this)
            {
                int compres;
                BinaryTreeNode<TKey, TValue> curnode = FindInsertionPoint(tofind, out compres);
                if ((compres == 0) && (curnode != null))
                {
                    found = curnode.value;
                    return true;
                }
            }
            found = default(TValue);
            return false;
        }

        internal bool FindNode(TKey tofind, out BinaryTreeNode<TKey,TValue> found)
        {
            lock (this)
            {
                int compres;
                BinaryTreeNode<TKey, TValue> curnode = FindInsertionPoint(tofind, out compres);
                if ((compres == 0) && (curnode != null))
                {
                    found = curnode;
                    return true;
                }
            }
            found = null;
            return false;
        }

        public TValue Next(TKey current)
        {
            lock (this)
            {
                BinaryTreeNode<TKey, TValue> curnode;
                if (FindNode(current, out curnode))
                    if ((curnode = InOrderNext(curnode)) != null)
                        return curnode.value;
                return default(TValue);
            }
        }

        public TValue Previous(TKey current)
        {
            lock (this)
            {
                BinaryTreeNode<TKey, TValue> curnode;
                if (FindNode(current, out curnode))
                    if ((curnode = InOrderPrevious(curnode)) != null)
                        return curnode.value;
                return default(TValue);
            }
        }



        private BinaryTreeNode<TKey, TValue> FindReplacement(BinaryTreeNode<TKey, TValue> ofnode)
        {
            BinaryTreeNode<TKey, TValue> toreturn;
            if (ofnode.rightchild != null)//leftmost in the right subtree
            {
                toreturn = ofnode.rightchild;
                while (toreturn.leftchild != null)
                    toreturn = toreturn.leftchild;
                return toreturn;
            }
            else if (ofnode.leftchild != null)//rightmost in the left subtree
            {
                toreturn = ofnode.leftchild;
                while (toreturn.rightchild != null)
                    toreturn = toreturn.rightchild;
                return toreturn;
            }
            else
                return ofnode;
        }

        private void DeleteLeafNode(BinaryTreeNode<TKey, TValue> todelete)
        {
            if (todelete.parent == null)
                root = null;
            else
            {
                if (todelete.parent.leftchild == todelete)
                    todelete.parent.leftchild = null;
                else if (todelete.parent.rightchild == todelete)
                    todelete.parent.rightchild = null;
                BalanceUp(todelete.parent);
            }
            m_Count--;
            return;
        }

        private void SwapNodes(BinaryTreeNode<TKey, TValue> node1, BinaryTreeNode<TKey, TValue> node2)
        {
            TKey key1 = node1.key;
            TValue val1 = node1.value;
            node1.key = node2.key;
            node1.value = node2.value;
            node2.key = key1;
            node2.value = val1;
            return;
        }

        public TValue Remove(TKey toremove)
        {
            int compres;
            BinaryTreeNode<TKey, TValue> swapnode;
            BinaryTreeNode<TKey, TValue> curnode;
            TValue toreturn;

            lock(this)
            {

                curnode = FindInsertionPoint(toremove, out compres);
                if ((curnode == null) || (compres != 0))
                {
                    return default(TValue);
                }
                toreturn = curnode.value;

                while ((curnode.leftchild != null) || (curnode.rightchild != null))
                {
                    swapnode = FindReplacement(curnode);
                    SwapNodes(curnode, swapnode);
                    curnode = swapnode;
                }

                DeleteLeafNode(curnode);

                return toreturn;
            }
        }

        internal BinaryTreeNode<TKey, TValue> leftmost()
        {
            lock(this)
            {
                BinaryTreeNode<TKey, TValue> toreturn = root;
                while ((toreturn != null) && (toreturn.leftchild != null))
                    toreturn = toreturn.leftchild;
                return toreturn;
            }
        }

        internal BinaryTreeNode<TKey, TValue> rightmost()
        {
            lock(this)
            {
                BinaryTreeNode<TKey, TValue> toreturn = root;
                while ((toreturn != null) && (toreturn.rightchild != null))
                    toreturn = toreturn.rightchild;
                return toreturn;
            }
        }

        internal BinaryTreeNode<TKey, TValue> InOrderNext(BinaryTreeNode<TKey, TValue> ofnode)
        {
            lock(this)
            {
                BinaryTreeNode<TKey, TValue> curnode;

                if (ofnode.rightchild != null)
                {
                    curnode = ofnode.rightchild;
                    while (curnode.leftchild != null)
                        curnode = curnode.leftchild;
                    return curnode;
                }
                else
                {
                    curnode = ofnode;
                    while (curnode.parent != null)
                    {
                        if (curnode.parent.leftchild == curnode)
                        {
                            return curnode.parent;
                        }
                        else
                            curnode = curnode.parent;
                    }
                    return null;
                }
            }
        }

        internal BinaryTreeNode<TKey, TValue> InOrderPrevious(BinaryTreeNode<TKey, TValue> ofnode)
        {
            lock(this)
            {
                BinaryTreeNode<TKey, TValue> curnode;

                if (ofnode.leftchild != null)
                {
                    curnode = ofnode.leftchild;
                    while (curnode.rightchild != null)
                        curnode = curnode.rightchild;
                    return curnode;
                }
                else
                {
                    curnode = ofnode;
                    while (curnode.parent != null)
                    {
                        if (curnode.parent.rightchild == curnode)
                        {
                            return curnode.parent;
                        }
                        else
                            curnode = curnode.parent;
                    }
                    return null;
                }
            }
        }

        public List<TValue> ToList()
        {
            lock(this)
            {
                List<TValue> toreturn = new List<TValue>();
                BinaryTreeNode<TKey, TValue> curnode = leftmost();
                while (curnode != null)
                {
                    toreturn.Add(curnode.value);
                    curnode = InOrderNext(curnode);
                }
                return toreturn;
            }
        }

        #region IEnumerable<TValue> Members

        public IEnumerator<TValue> GetEnumerator()
        {
            return new BinaryTreeEnumerator<TValue>(ToList());
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new BinaryTreeEnumerator<TValue>(ToList());
        }

        #endregion

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            Insert(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            lock(this)
            {
                int compres;
                BinaryTreeNode<TKey, TValue> curnode = FindInsertionPoint(key, out compres);
                if ((compres == 0) && (curnode != null))
                {
                    return true;
                }
                return false;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> toreturn = new List<TKey>();
                lock(this)
                {
                    BinaryTreeNode<TKey, TValue> curnode = leftmost();
                    while (curnode != null)
                    {
                        toreturn.Add(curnode.key);
                        curnode = InOrderNext(curnode);
                    }
                }
                return toreturn;
            }
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            int compres;
            BinaryTreeNode<TKey, TValue> swapnode;
            BinaryTreeNode<TKey, TValue> curnode;
            TValue toreturn;

            lock(this)
            {

                curnode = FindInsertionPoint(key, out compres);
                if ((curnode == null) || (compres != 0))
                {
                    return false;
                }
                toreturn = curnode.value;

                while ((curnode.leftchild != null) || (curnode.rightchild != null))
                {
                    swapnode = FindReplacement(curnode);
                    SwapNodes(curnode, swapnode);
                    curnode = swapnode;
                }

                DeleteLeafNode(curnode);

                return true;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Find(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return ToList(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue toreturn;
                if (Find(key, out toreturn))
                    return toreturn;
                else
                    return default(TValue);
            }
            set
            {
                Insert(key, value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Insert(item.Key, item.Value);
        }

        public void Clear()
        {
            BinaryTreeNode<TKey, TValue> curnode;
            curnode = leftmost();
            while (curnode != null)
            {
                Remove(curnode.key);
                curnode = leftmost();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count
        {
            get { return (int)Count; }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)this).Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            List<KeyValuePair<TKey, TValue>> kvplist = new List<KeyValuePair<TKey, TValue>>();
            lock(this)
            {
                BinaryTreeNode<TKey, TValue> curnode;

                curnode = leftmost();
                while (curnode != null)
                {
                    kvplist.Add(new KeyValuePair<TKey, TValue>(curnode.key, curnode.value));
                    curnode = InOrderNext(curnode);
                }

            }
            return kvplist.GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            string toreturn="";
            foreach (KeyValuePair<TKey, TValue> kvp in (IEnumerable<KeyValuePair<TKey, TValue>>)this)
                toreturn += "\t" + kvp.ToString() + "\n";
            return toreturn;
        }
    }
}
