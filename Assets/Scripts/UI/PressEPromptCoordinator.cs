using System.Collections.Generic;
using UnityEngine;

public static class PressEPromptCoordinator
{
    private static readonly Dictionary<GameObject, HashSet<int>> ActiveRequestersByUI = new Dictionary<GameObject, HashSet<int>>();
    private static readonly Dictionary<int, GameObject> UIByRequester = new Dictionary<int, GameObject>();

    public static void SetRequest(GameObject ui, Object requester, bool shouldShow)
    {
        if (ui == null || requester == null)
        {
            return;
        }

        int requesterId = requester.GetInstanceID();

        if (!ActiveRequestersByUI.TryGetValue(ui, out HashSet<int> requesters))
        {
            requesters = new HashSet<int>();
            ActiveRequestersByUI[ui] = requesters;
        }

        if (shouldShow)
        {
            requesters.Add(requesterId);
            UIByRequester[requesterId] = ui;
        }
        else
        {
            requesters.Remove(requesterId);
            UIByRequester.Remove(requesterId);
        }

        if (requesters.Count == 0)
        {
            ActiveRequestersByUI.Remove(ui);
        }

        bool hasAnyRequester = ActiveRequestersByUI.TryGetValue(ui, out HashSet<int> activeSet) && activeSet.Count > 0;
        if (ui.activeSelf != hasAnyRequester)
        {
            ui.SetActive(hasAnyRequester);
        }
    }

    public static void ClearRequester(Object requester)
    {
        if (requester == null)
        {
            return;
        }

        int requesterId = requester.GetInstanceID();
        if (!UIByRequester.TryGetValue(requesterId, out GameObject ui) || ui == null)
        {
            UIByRequester.Remove(requesterId);
            return;
        }

        if (ActiveRequestersByUI.TryGetValue(ui, out HashSet<int> requesters))
        {
            requesters.Remove(requesterId);
            if (requesters.Count == 0)
            {
                ActiveRequestersByUI.Remove(ui);
                if (ui.activeSelf)
                {
                    ui.SetActive(false);
                }
            }
            else if (!ui.activeSelf)
            {
                ui.SetActive(true);
            }
        }

        UIByRequester.Remove(requesterId);
    }
}
