using System;
using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Owns the order queue / concurrent active orders, pick + stack progress,
    /// verification + rework, SLA timers, load + completion, and endless waves.
    /// Driven by a <see cref="LevelConfig"/>. See CLAUDE_CODE_SPEC.md
    /// Sections 13.4 / 15 / 24 / 26 / 27 and 6.2.
    /// </summary>
    public class OrderManager : MonoBehaviour
    {
        private const int OrdersPerWave = 3;
        private const int EndlessLives = 3;

        public bool RequiresStacking { get; private set; }
        public bool RequiresVerification { get; private set; }
        public bool Endless { get; private set; }
        public int MaxConcurrent { get; private set; } = 1;

        private readonly List<CustomerOrder> active = new List<CustomerOrder>();
        private readonly Queue<CustomerOrder> pending = new Queue<CustomerOrder>();
        private readonly Dictionary<string, Pallet> palletByOrder = new Dictionary<string, Pallet>();
        private readonly Dictionary<string, float> slaRemaining = new Dictionary<string, float>();
        private readonly HashSet<string> imperfect = new HashSet<string>();
        private readonly List<Job> jobs = new List<Job>();

        private int selectedIndex;
        private int targetCompletions;
        private float slaSeconds;

        private Tote tote;
        private SkuCatalog catalog;
        private Func<CustomerOrder, Pallet> palletFactory;
        private Func<CustomerOrder> orderFactory;

        public ScoreSystem Score { get; private set; }
        public IReadOnlyList<CustomerOrder> ActiveOrders => active;
        public int CompletedCount { get; private set; }
        public int FailedCount { get; private set; }
        public int ReworkJobsCreated { get; private set; }
        public int Wave => CompletedCount / OrdersPerWave + 1;

        public CustomerOrder SelectedOrder =>
            active.Count > 0 ? active[Mathf.Clamp(selectedIndex, 0, active.Count - 1)] : null;
        public CustomerOrder ActiveOrder => SelectedOrder;
        public int SelectedIndex => selectedIndex;

        public int ActiveReworkJobs
        {
            get
            {
                int c = 0;
                foreach (var j in jobs)
                    if (j.Type == JobType.Rework && j.State == JobState.Active) c++;
                return c;
            }
        }

        public event Action OrdersChanged;
        public event Action<CustomerOrder> OrderCompleted;
        public event Action<CustomerOrder> OrderFailed;
        public event Action AllOrdersDone;     // campaign win
        public event Action EndlessEnded;      // endless out of lives
        public event Action<VerificationResult> VerificationReported;

        // ---------------------------------------------------------------- setup
        public void Init(Tote tote, ScoreSystem score, SkuCatalog catalog, LevelConfig config,
                         Func<CustomerOrder, Pallet> palletFactory, Func<CustomerOrder> orderFactory)
        {
            BindTote(tote);
            Score = score;
            this.catalog = catalog;
            this.palletFactory = palletFactory;
            this.orderFactory = orderFactory;

            RequiresStacking = config.UseStacking;
            RequiresVerification = config.UseVerification;
            Endless = config.Endless;
            MaxConcurrent = Mathf.Max(1, config.MaxConcurrent);
            targetCompletions = config.OrderCount;
            slaSeconds = config.SlaSeconds;
        }

        public void BindTote(Tote t)
        {
            if (tote != null) tote.Changed -= OnToteChanged;
            tote = t;
            if (tote != null) tote.Changed += OnToteChanged;
        }

        private void OnDestroy()
        {
            if (tote != null) tote.Changed -= OnToteChanged;
        }

        private void OnToteChanged() => OrdersChanged?.Invoke();

        private int orderCounter;
        public CustomerOrder MakeOrder(IList<OrderLine> lines, string dockLaneId, float dueSeconds = 0f)
        {
            orderCounter++;
            return new CustomerOrder
            {
                OrderId = $"ORD-{orderCounter:000}",
                Priority = 1,
                DockLaneId = dockLaneId,
                DueTimeSeconds = dueSeconds,
                State = OrderState.Pending,
                Lines = new List<OrderLine>(lines)
            };
        }

        public void Enqueue(CustomerOrder order) => pending.Enqueue(order);

        public void Begin()
        {
            FillActive();
            OrdersChanged?.Invoke();
        }

        private void FillActive()
        {
            while (active.Count < MaxConcurrent)
            {
                CustomerOrder o = null;
                if (Endless) o = orderFactory != null ? orderFactory() : null;
                else if (pending.Count > 0) o = pending.Dequeue();
                if (o == null) break;

                o.State = OrderState.Picking;
                active.Add(o);
                slaRemaining[o.OrderId] = slaSeconds > 0f ? slaSeconds : -1f;
                if (RequiresStacking && palletFactory != null)
                {
                    var p = palletFactory(o);
                    if (p != null) palletByOrder[o.OrderId] = p;
                }
            }
        }

        public Pallet PalletFor(CustomerOrder order)
        {
            return order != null && palletByOrder.TryGetValue(order.OrderId, out var p) ? p : null;
        }

        public float GetSlaRemaining(CustomerOrder order)
        {
            return order != null && slaRemaining.TryGetValue(order.OrderId, out float r) ? r : -1f;
        }

        // ------------------------------------------------------------ selection
        public void CycleSelection()
        {
            if (active.Count <= 1) return;
            selectedIndex = (selectedIndex + 1) % active.Count;
            OrdersChanged?.Invoke();
        }

        // -------------------------------------------------------------- queries
        public int RequiredQuantity(CustomerOrder order, string skuId)
        {
            if (order == null || skuId == null) return 0;
            int t = 0;
            foreach (var l in order.Lines) if (l.SkuId == skuId) t += l.Quantity;
            return t;
        }

        public int Collected(CustomerOrder order, string skuId)
        {
            if (order == null || skuId == null) return 0;
            if (RequiresStacking)
            {
                var p = PalletFor(order);
                return p != null ? p.Grid.Count(skuId) : 0;
            }
            return tote != null ? tote.Inventory.GetQuantity(skuId) : 0;
        }

        public bool AllLinesCollected(CustomerOrder order)
        {
            if (order == null) return false;
            foreach (var l in order.Lines)
                if (Collected(order, l.SkuId) < l.Quantity) return false;
            return true;
        }

        public bool IsReadyToLoad(CustomerOrder order)
        {
            if (order == null) return false;
            return RequiresVerification ? order.State == OrderState.Verified : AllLinesCollected(order);
        }

        public bool IsReadyToLoad() => IsReadyToLoad(SelectedOrder);

        public bool AnyActiveOrderNeeds(string skuId)
        {
            int inTote = tote != null ? tote.Inventory.GetQuantity(skuId) : 0;
            int needed = 0;
            foreach (var o in active)
            {
                int req = RequiredQuantity(o, skuId);
                if (req <= 0) continue;
                if (RequiresStacking)
                {
                    var p = PalletFor(o);
                    int onPallet = p != null ? p.Grid.Count(skuId) : 0;
                    int rem = req - onPallet;
                    if (rem > 0) needed += rem;
                }
                else
                {
                    int rem = req - inTote;
                    if (rem > 0) needed += rem;
                }
            }
            return RequiresStacking ? needed > inTote : needed > 0;
        }

        // ------------------------------------------------------------- stacking
        public string NextStackableSku(CustomerOrder order)
        {
            if (order == null || tote == null) return null;
            foreach (var l in order.Lines)
                if (Collected(order, l.SkuId) < l.Quantity && tote.Inventory.GetQuantity(l.SkuId) > 0)
                    return l.SkuId;
            return null;
        }

        public void OnStacked(CustomerOrder order, string skuId)
        {
            tote?.Remove(skuId, 1);
            OrdersChanged?.Invoke();
        }

        public void OnIllegalStack()
        {
            Score?.IllegalStack();
            OrdersChanged?.Invoke();
        }

        // --------------------------------------------------------------- verify
        public VerificationResult Verify(CustomerOrder order)
        {
            var result = new VerificationResult { OrderId = order?.OrderId };
            if (order == null) return result;

            bool pass = true;
            foreach (var l in order.Lines)
            {
                int have = Collected(order, l.SkuId);
                int missing = Mathf.Max(0, l.Quantity - have);
                result.Lines.Add(new VerificationResult.Line
                {
                    SkuId = l.SkuId,
                    Required = l.Quantity,
                    Collected = have,
                    Missing = missing
                });
                if (missing > 0) pass = false;
            }

            if (RequiresStacking)
            {
                var p = PalletFor(order);
                if (p != null)
                    foreach (var kv in p.Grid.Contents())
                        if (RequiredQuantity(order, kv.Key) == 0)
                        {
                            result.WrongItems += kv.Value;
                            pass = false;
                        }
            }

            result.Passed = pass;
            if (pass)
            {
                order.State = OrderState.Verified;
                CompleteReworkJobs(order);
            }
            else
            {
                order.State = OrderState.VerificationFailed;
                imperfect.Add(order.OrderId);
                CreateReworkJob(order);
                Score?.FailedVerification();
            }

            VerificationReported?.Invoke(result);
            OrdersChanged?.Invoke();
            return result;
        }

        private void CreateReworkJob(CustomerOrder order)
        {
            foreach (var j in jobs)
                if (j.OrderId == order.OrderId && j.Type == JobType.Rework && j.State == JobState.Active)
                    return;

            jobCounter++;
            ReworkJobsCreated++;
            jobs.Add(new Job
            {
                JobId = $"RWK-{jobCounter:000}",
                Type = JobType.Rework,
                OrderId = order.OrderId,
                State = JobState.Active
            });
        }
        private int jobCounter;

        private void CompleteReworkJobs(CustomerOrder order)
        {
            foreach (var j in jobs)
                if (j.OrderId == order.OrderId && j.Type == JobType.Rework && j.State == JobState.Active)
                    j.State = JobState.Completed;
        }

        // ----------------------------------------------------------------- load
        public bool TryLoad(CustomerOrder order, string dockLaneId, out bool wrongLane)
        {
            wrongLane = false;
            if (order == null || !IsReadyToLoad(order)) return false;

            if (order.DockLaneId != dockLaneId)
            {
                wrongLane = true;
                imperfect.Add(order.OrderId);
                Score?.WrongDock();
                OrdersChanged?.Invoke();
                return false;
            }

            Complete(order);
            return true;
        }

        private void Complete(CustomerOrder order)
        {
            order.State = OrderState.Loaded;
            bool perfect = !imperfect.Contains(order.OrderId);
            Score?.CompleteOrder(perfect, 0f);

            CleanupOrder(order);
            CompletedCount++;

            if (!Endless && CompletedCount >= targetCompletions)
            {
                OrdersChanged?.Invoke();
                AllOrdersDone?.Invoke();
                return;
            }

            FillActive();
            OrderCompleted?.Invoke(order);
            OrdersChanged?.Invoke();
        }

        private void FailOrder(CustomerOrder order)
        {
            order.State = OrderState.Failed;
            Score?.MissedSla();
            CleanupOrder(order);
            FailedCount++;

            if (Endless && FailedCount >= EndlessLives)
            {
                OrdersChanged?.Invoke();
                EndlessEnded?.Invoke();
                return;
            }

            if (!Endless && orderFactory != null)
                pending.Enqueue(orderFactory()); // replacement so the target stays reachable

            FillActive();
            OrderFailed?.Invoke(order);
            OrdersChanged?.Invoke();
        }

        private void CleanupOrder(CustomerOrder order)
        {
            if (palletByOrder.TryGetValue(order.OrderId, out var p))
            {
                if (p != null) Destroy(p.gameObject);
                palletByOrder.Remove(order.OrderId);
            }
            else if (!RequiresStacking && tote != null)
            {
                foreach (var l in order.Lines) tote.Remove(l.SkuId, l.Quantity);
            }
            slaRemaining.Remove(order.OrderId);
            imperfect.Remove(order.OrderId);
            active.Remove(order);
            if (selectedIndex >= active.Count) selectedIndex = Mathf.Max(0, active.Count - 1);
        }

        // ----------------------------------------------------------------- tick
        private readonly List<CustomerOrder> expired = new List<CustomerOrder>();

        private void Update()
        {
            if (slaSeconds <= 0f || active.Count == 0) return;

            expired.Clear();
            float dt = Time.deltaTime;
            foreach (var o in active)
            {
                if (!slaRemaining.TryGetValue(o.OrderId, out float r) || r < 0f) continue;
                r -= dt;
                slaRemaining[o.OrderId] = r;
                if (r <= 0f) expired.Add(o);
            }
            foreach (var o in expired) FailOrder(o);
        }
    }
}
