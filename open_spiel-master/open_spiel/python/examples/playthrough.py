# Copyright 2019 DeepMind Technologies Ltd. All rights reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

"""Play a game, selecting random moves, and save what we see.

This can be used to check by hand the behaviour of a game, and also
as the basis for test cases.

Example usage:
```
playthrough --game kuhn_poker --params players=3
```
"""

from __future__ import absolute_import
from __future__ import division
from __future__ import print_function

from absl import app
from absl import flags

from open_spiel.python.algorithms import generate_playthrough

FLAGS = flags.FLAGS

flags.DEFINE_string(
    "game", "kuhn_poker", "Name of the game, with optional parameters, e.g. "
    "'kuhn_poker' or 'go(komi=4.5,board_size=19)'.")
flags.DEFINE_string("output_file", None, "Where to write the data to.")
flags.DEFINE_integer("seed", None,
                     "The random-number seed to use to select actions.")

flags.DEFINE_string("update_path", None,
                    "If set, regenerates all playthroughs in the path.")
flags.DEFINE_bool(
    "alsologtostdout", False,
    "If True, the trace will be written to std-out while it "
    "is being constructed (in addition to the usual behavior).")


def main(unused_argv):
  if FLAGS.update_path:
    generate_playthrough.update_path(FLAGS.update_path)
  else:
    if not FLAGS.game:
      raise ValueError("Must specify game")
    text = generate_playthrough.playthrough(
        FLAGS.game, FLAGS.seed, alsologtostdout=FLAGS.alsologtostdout)
    if FLAGS.output_file:
      with open(FLAGS.output_file, "w") as f:
        f.write(text)
    else:
      print(text, end="")


if __name__ == "__main__":
  app.run(main)
