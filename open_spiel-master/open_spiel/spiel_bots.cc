// Copyright 2019 DeepMind Technologies Ltd. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#include "open_spiel/spiel_bots.h"

#include <algorithm>
#include <unordered_set>
#include <vector>

#include "open_spiel/abseil-cpp/absl/random/uniform_int_distribution.h"
#include "open_spiel/spiel.h"
#include "open_spiel/spiel_utils.h"

namespace open_spiel {
namespace {

class UniformRandomBot : public Bot {
 public:
  UniformRandomBot(Player player_id, int seed)
      : player_id_(player_id), rng_(seed) {}
  ~UniformRandomBot() = default;

  void RestartAt(const State&) {}
  Action Step(const State& state) override {
    return StepWithPolicy(state).second;
  }
  bool ProvidesPolicy() override { return true; }
  ActionsAndProbs GetPolicy(const State& state) override {
    ActionsAndProbs policy;
    auto legal_actions = state.LegalActions(player_id_);
    const int num_legal_actions = legal_actions.size();
    const double p = 1.0 / num_legal_actions;
    for (auto action : legal_actions) policy.emplace_back(action, p);
    return policy;
  }

  std::pair<ActionsAndProbs, Action> StepWithPolicy(
      const State& state) override {
    ActionsAndProbs policy = GetPolicy(state);
    const int num_legal_actions = policy.size();

    int selection =
        absl::uniform_int_distribution<int>(0, num_legal_actions - 1)(rng_);
    return std::make_pair(policy, policy[selection].first);
  }

 private:
  const Player player_id_;
  std::mt19937 rng_;
};

class PolicyBot : public Bot {
 public:
  PolicyBot(int seed, std::unique_ptr<Policy> policy)
      : Bot(), rng_(seed), policy_(std::move(policy)) {}
  ~PolicyBot() = default;

  void RestartAt(const State&) {}
  Action Step(const State& state) override {
    return StepWithPolicy(state).second;
  }
  bool ProvidesPolicy() override { return true; }
  ActionsAndProbs GetPolicy(const State& state) override {
    return policy_->GetStatePolicy(state);
  }

  std::pair<ActionsAndProbs, Action> StepWithPolicy(
      const State& state) override {
    ActionsAndProbs actions_and_probs = GetPolicy(state);
    Action action =
        SampleAction(actions_and_probs,
                     std::uniform_real_distribution<double>(0.0, 1.0)(rng_))
            .first;
    return {actions_and_probs, action};
  }

 private:
  std::mt19937 rng_;
  std::unique_ptr<Policy> policy_;
};

}  // namespace

// A uniform random bot, for test purposes.
std::unique_ptr<Bot> MakeUniformRandomBot(Player player_id, int seed) {
  return std::unique_ptr<Bot>(new UniformRandomBot(player_id, seed));
}

// A bot that samples from a policy.
std::unique_ptr<Bot> MakePolicyBot(const Game& game, Player player_id, int seed,
                                   std::unique_ptr<Policy> policy) {
  return std::make_unique<PolicyBot>(seed, std::move(policy));
}

namespace {
class FixedActionPreferenceBot : public Bot {
 public:
  FixedActionPreferenceBot(Player player_id, const std::vector<Action>& actions)
      : Bot(), player_id_(player_id), actions_(actions) {}
  ~FixedActionPreferenceBot() = default;

  void RestartAt(const State&) {}
  Action Step(const State& state) override {
    return StepWithPolicy(state).second;
  }
  bool ProvidesPolicy() override { return true; }
  ActionsAndProbs GetPolicy(const State& state) {
    std::vector<Action> legal_actions = state.LegalActions(player_id_);
    std::unordered_set<Action> legal_actions_set =
        std::unordered_set<Action>(legal_actions.begin(), legal_actions.end());
    for (Action action : actions_) {
      if (legal_actions_set.count(action) == 1) {
        return {{action, 1.0}};
      }
    }
    SpielFatalError("No legal actions in action list.");
  }

  std::pair<ActionsAndProbs, Action> StepWithPolicy(
      const State& state) override {
    ActionsAndProbs actions_and_probs = GetPolicy(state);
    return {actions_and_probs, actions_and_probs[0].first};
  }

 private:
  const Player player_id_;
  std::vector<Action> actions_;
};

}  // namespace

// A bot with a fixed action preference, for test purposes.
// Picks the first legal action found in the list of actions.
std::unique_ptr<Bot> MakeFixedActionPreferenceBot(
    Player player_id, const std::vector<Action>& actions) {
  return std::unique_ptr<Bot>(new FixedActionPreferenceBot(player_id, actions));
}

}  // namespace open_spiel
