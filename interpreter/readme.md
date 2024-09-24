# Forth Interpreter (Python)

## Setup

* Install Python: `apt-get install python`
* Interpreter: `python ./interpreter.py`
* Execute source: `cat foo.f bar.f | python ./interpreter.py`
* Execute source followed by interactive (add `-`): `cat foo.f bar.f - | python ./interpreter.py`

## Vocabulary

The following primitive words are available:

| Word    | Signature | Description                          |
| ------- | --------- | ------------------------------------ |
| `+`     |  yx-n     | Add                                  |
| `-`     |  yx-n     | Subtract                             |
| `*`     |  yx-n     | Multiply                             |
| `/`     |  yx-n     | Divide                               |
| `mod`   |   x-n     | Modulus                              |
| `cos`   |   x-n     | Cosine                               |
| `sin`   |   x-n     | Sine                                 |
| `tan`   |   x-n     | Tangent                              |
| `acos`  |   x-n     | Arc cosine                           |
| `asin`  |   x-n     | Arc sine                             |
| `atan`  |   x-n     | Arc tangent                          |
| `floor` |   x-n     | Floor number                         |
| `.`     |    -      | Pop and print top value              |
| `.s`    |    -      | Print whole stack                    |
| `=`     |  yx-b     | Equal                                |
| `<>`    |  yx-b     | Not equal                            |
| `>`     |  yx-b     | Greater than                         |
| `>=`    |  yx-b     | Greater than or equal                |
| `<`     |  yx-b     | Less than                            |
| `<=`    |  yx-b     | Less than or equal                   |
| `and`   |  yx-b     | Logical/bitwise and                  |
| `or`    |  yx-b     | Logical/bitwise or                   |
| `xor`   |  yx-b     | Logical/bitwise xor                  |
| `not`   |   x-b     | Logical/bitwise not                  |
| `dup`   |  yx-b     | Duplicate top value                  |
| `drop`  |   x-      | Drop top value                       |
| `swap`  |  yx-xy    | Swap top two values                  |
| `over`  |  yx-xyx   | Duplicate 2nd value to top           |
| `rot`   | zyx-xzy   | Rotate 3rd value to top              |
| `-rot`  | zyx-yxz   | Rotate top value to 3rd              |
| `var`   |    -      | Create variable                      |
| `@`     |   a-n     | Fetch variable value                 |
| `!`     |  xa-      | Store into variable                  |
| `const` |   x-      | Create constant                      |
| `m@`    |   a-n     | Fetch memory address                 |
| `m!`    |  xa-      | Store into memory address            |
| `.m`    |    -      | Print memory range                   |
| `dump`  |    -      | Dump memory to image file            |
| `(`     |    -      | Start comment (to `)`)               |
| `if`    |    -      | Start conditional (to `else`/`then`) |
| `else`  |    -      | Start alternative (to `then`)        |
| `then`  |    -      | End conditional                      |
| `do`    |    -      | Start loop (to `loop`)               |
| `i`     |    -i     | Get loop index                       |
| `:`     |    -      | Start definition (to `;`)            |
| `'`     |    -a     | Get address of next token            |
| `[:`    |    -      | Start lambda (to `:]`)               |
| `call`  |   a-      | Call address                         |
| `emit`  |   x-      | Emit to console                      |
| `sym`   |    -c..cn | Deconstruct next token               |
| `words` |    -      | Print dictionary words               |
| `exit`  |    -      | Halt interpreter                     |

## Walkthrough

This is a stepping stone on our way to more interesting interpreters. The core Forth interpreter is in this `Forth` class defined in [interpreter.py](./interpreter.py):

```python
class Forth:
  def __init__(self):
    ...
```

### REPL

Ultimately, the `Forth` interpreter will be constructed and driven in an interactive REPL loop by calling `read()`, `evaluate()` and any results are printed followed by _ok_.

```python
forth = Forth()

print("Welcome to PyForth REPL")
while True:
  try:
    print('> ', end='')
    forth.read()
    forth.evaluate()
    print('ok')
  except EOFError:
    print('done')
    exit()
  except Exception as error: print(error)
```

We `read()` whitespace-separated tokens from the console. The input may be typed at the console, or may be piped from files and/or keyboard input (e.g. to load a file and then proceed interactively `cat foo.f - | python ./interpreter.py`).

```python
  def read(self):
    self.tokens = (token for token in input().split())
```

This is the whole initial lexer and parser! It cannot be simpler. The stream of tokens hang off of `tokens`. Evaluation of each token is quite straight forward, making use of the hallmark features of Forth, which are a stack and a dictionary. The `stack` will contain arguments waiting to be consumed by _words_, which are functions defined in the `dictionary`.

```python
    self.stack = []
    self.dictionary = {
        ... }
```

Values may be placed on and retrieved from the stack with `push()` and `pop()`.

```python
  def push(self, x): self.stack.append(x)

  def pop(self):
    if len(self.stack) == 0:
      raise Exception("Stack empty")
    self.stack, val = self.stack[:-1], self.stack[-1]
    return val
```

Evaluation peals off tokens one by one and handles them. First we try to find the token in the `dictionary` and call the associated function. If it's not in the dictionary then we try to parse it as a (`float`) number and push it onto the stack. If it's neither a defined word or a literal number then this is an error (`ValueError`). Any errors are printed by the REPL. For example entering `foo` (assuming _foo_ is not defined) prints `foo?` showing that the word is unknown, or `42 0 /` prints `/ error float division by zero`.

```python
  def evaluate(self):
    try:
      while True:
        token = next(self.tokens)
        if token in self.dictionary:
          self.dictionary[token]()
        else:
          self.push(float(token))
    except ValueError: raise Exception(f'{token}?') # not found
    except StopIteration: pass # end of tokens
    except Exception as error: raise Exception(f'{token} error {error}')
```

Fun fact: Because lookup happens before parsing, it is actually possible to define words that look like numbers!

That's it for the _inner interpreter_. The rest comes down to adding useful word definitions to the dictionary.

### Math Operators

Let's start with some basic math operators.

```python
    self.dictionary = {
      '+'    : lambda: self.xx_x(operator.add),
      '-'    : lambda: self.xx_x(operator.sub),
      '*'    : lambda: self.xx_x(operator.mul),
      '/'    : lambda: self.xx_x(operator.truediv),
      'mod'  : lambda: self.xx_x(operator.mod),
      'cos'  : lambda: self.x_x(math.cos),
      'sin'  : lambda: self.x_x(math.sin),
      'tan'  : lambda: self.x_x(math.tan),
      'acos' : lambda: self.x_x(math.acos),
      'asin' : lambda: self.x_x(math.asin),
      'atan' : lambda: self.x_x(math.atan),
      'floor': lambda: self.x_x(math.floor),
      ... }
```

Notice that binary operations taking two arguments and returning a single result are wrapped in `xx_x(...)` while unary operations are wrapped in `x_x`. These are helper functions to `pop()` arguments and `push()` results.

```python
  def push2(self, xy):
    x, y = xy
    self.push(x)
    self.push(y)
  def push3(self, xyz):
    x, y, z = xyz
    self.push(x)
    self.push(y)
    self.push(z)

  def flip2(self, f, x, y): return f(y, x)
  def flip3(self, f, x, y, z): return f(z, y, x)
  def x_x(self, f): self.push(f(self.pop()))
  def xx_x(self, f): self.push(self.flip2(f, self.pop(), self.pop()))
  def xx_b(self, f): self.push(-1 if self.flip2(f,self.pop(),self.pop()) else 0)
  def xx_xx(self, f): self.push2(self.flip2(f, self.pop(), self.pop()))
  def x_xx(self, f): self.push2(f(self.pop()))
  def xx_xxx(self, f): self.push3(self.flip2(f, self.pop(), self.pop()))
  def xxx_xxx(self, f): self.push3(self.flip3(f, self.pop(), self.pop(), self.pop()))
  def x_(self, f): f(self.pop())
  def _x(self, f): self.push(f())
  def xx_(self, f): self.flip2(f, self.pop(), self.pop())
```

The reason for the `flip2`/`flip3` functions is to rearrange the parameters being popped from the stack to match the infix operators so that, for example, `4 3 -` results in `1` rather than `-1`.

With math operators we can start to use the interpreter as an RPN calculator, expressions like `42 6 *` and `3.14 sin` will work; leaving the result on the stack.

To see the result, we can use `.` or can print the whole stack with `.s`. Note that `.` _consumes_ the top element while `.s` merely displays the values.

```python
    self.dictionary = {
      '.'    : lambda: print(self.pop()),
      '.s'   : lambda: print(self.stack),
      ... }
```

### Comparison Operators

We can add comparison operations.

```python
    self.dictionary = {
      '='    : lambda: self.xx_b(operator.eq),
      '<>'   : lambda: self.xx_b(operator.ne),
      '>'    : lambda: self.xx_b(operator.gt),
      '>='   : lambda: self.xx_b(operator.ge),
      '<'    : lambda: self.xx_b(operator.lt),
      '<='   : lambda: self.xx_b(operator.le),
      ... }
```

Note that the `xx_b` wrapper function returns a _boolean_. We use `-1` and `0` to represent _true_ and _false_. You can see that `xx_b` pushes `-1 if ... else 0`.

### Logical/Bitwise Operators

This leads us to the logical operators.

```python
    self.dictionary = {
      'and'  : lambda: self.xx_x(lambda x,y: int(x) & int(y)),
      'or'   : lambda: self.xx_x(lambda x,y: int(x) | int(y)),
      'xor'  : lambda: self.xx_x(lambda x,y: int(x) ^ int(y)),
      'not'  : lambda: self.x_x(lambda x: ~x),
      ... }
```

These appear to be _bitwise_ operators, and they are! They work as _both_ bitwise and logical operators because true (`-1`) is all bits set and false (`0`) is all bits reset. This unification simplifies the vocabulary of words. Compare this to languages like C with `&&`, `||`, `!`, ... vs. `&`, `|`, `~`, ... and most languages don't even have a logical `xor`.

### Stack Manipulation

To manipulate the positions of values on the stack, we'll add these standard Forth words.

```python
    self.dictionary = {
      'dup'  : lambda: self.x_xx(lambda x: (x,x)),
      'drop' : lambda: self.x_(lambda _: None),
      'swap' : lambda: self.xx_xx(lambda x,y: (y,x)),
      'over' : lambda: self.xx_xxx(lambda x,y: (x,y,x)),
      'rot'  : lambda: self.xxx_xxx(lambda x,y,z: (y,z,x)),
      '-rot' : lambda: self.xxx_xxx(lambda x,y,z: (z,x,y)),
      ... }
```

We can duplicate the top value (e.g. `7.2 dup *` produces the square `51.84`), or `drop` it. We can `swap` the top two values. We can use `over` to copy the second value over the top of the stack (e.g. `7 42 over .s` shows `7 42 7`) . The `rot` and `-rot` words rotate the top three element (e.g. `1 2 3 rot` -> `2 3 1` and `1 2 3 -rot` -> `3 1 2`).

### Variables

An area is set aside for `var` storage. We may create variables with `var foo` and then store (`!`) into them with `42 foo !` and fetch (`@`) their values with `foo @`.

```python
    self.variables = []

    ...

    self.dictionary = {
      'var'  : self.variable,
      '@'    : lambda: self.x_x(lambda x: self.variables[int(x)]),
      '!'    : lambda: self.xx_(self.variableStore),
      ... }
```

This is quite different to how variables normally work in Forth. Normally a variable is merely a memory address and fetch (`@`) and store (`!`) are general operators for addressing memory. We want to keep the moving parts cleanly separated for now.

The `var` word creates a variable by the name of the following token (e.g. `var something`). It does this by getting the next token as the name. This is the first of several words that interact with parsing by intercepting tokens before `evaluate()` sees them. So this adds _syntax_ to the language: `var {name}`. It created a slot in `self.variables` and also adds a word to the dictionary that pushes the index of the variable.

```python
  def variable(self):
    name = next(self.tokens)
    index = len(self.variables)
    self.variables.append(0)
    self.dictionary[name] = lambda: self.push(index)
```

This means that `var foo` creates the variable slot and word, while `foo` thereafter pushes the slot index. Fetch (`@`) then takes a slot number so `foo @` will fetch the value. Store (`!`) takes a value and a slot index and sets the slot's value, so the expression `42 foo !` pushes the value (`42`) followed by the slot number (by calling `foo`) and fetch (`!`) retrieves it.

By the way, the standard Forth word is `variable` rather than `var` but I like the shorter version. Chuck Moore's Forths have never followed a standard and nor do ours. Make it your own. Liberating!

Storing the value is a separate function because lambda expressions cannot do assignment directly in Python.

```python
  def variableStore(self, val, addr): self.variables[addr] = val
```

### Constants

Constants can be defined like `3.1415926535 const pi`.

```python
    self.dictionary = {
      'const': self.constant,
      ... }
```

Similar to `var`, it takes the next token as the name, along with the previously pushed value. More language syntax: `{value} const {name}`. The value may be computed at _compile time_ as in `pi 180.0 / const rads` to define a factor for conversion of degrees to radians.

```python
  def constant(self):
    name = next(self.tokens)
    val = self.pop()
    self.dictionary[name] = lambda: self.push(val)
```

It may look a little tricky, but the `lambda` is a closure over the constant `val`, which it pushes when called.

### Memory

Remember that this interpreter is temporary. We will play with it a bit, but primarily it will be used to build the initial seed image for _hardware_ (a VM written in C) from which we will migrate to a more _native_ Forth. For building the image, we will use a memory space:

```python
    self.memory = [0] * 32 * 1024
```

We will see later that structures such as the stack, dictionary and variables in a native Forth are merely built in raw memory. Here we're making these structures separately concrete and visible in the interpreter so that we can more clearly see what they represent. So `memory` remains separate and available exclusively for building the seed image.

Similar to variables, we have a memory-fetch (`m@`) and memory-store (`m!`). If you're used to regular Forth where there is only `@` and `!` this will seem weird! Because we're keeping variables and raw memory separate we need separate operators, for now.

To store a `42` into address `123`, we say `42 123 m!`. To retrieve it we say `123 m@`.

To take a look at a portion of memory we say something like `130 120 .m` which will show ten cells from address 120-130 (including the value we just stored).

Finally, we can `dump` all of memory to an `image.bin` file. This will be used to create the seed image for our VM in the future. 

```python
    self.dictionary = {
      'm@'   : lambda: self.x_x(lambda x: self.memory[int(x)]),
      'm!'   : lambda: self.xx_(self.memoryStore),
      '.m'   : lambda: print(self.memory[int(self.pop()):int(self.pop())]),
      'dump' : self.dump,
      ... }
```

Again like with `variableStore`, `memoryStore` is a separate function because lambda expressions cannot do assignment directly in Python.

```python
  def memoryStore(self, val, addr): self.memory[int(addr)] = int(val)
```

Dumping to a image file is straight forward. Memory cells are packed as little-endian 2-byte signed `short` values.

```python
  def dump(self):
    with open('image.bin', 'wb') as f:
      for m in self.memory:
        f.write(struct.pack('h', m))
```

### Comments

Comments are in the form `( this is a comment )`, with `(` being another syntax word.

```python
    self.dictionary = {
      '('    : self.comment,
      ... }
```

The `(` word starts a comment and `)` terminates. All that the word does is scan past all of the tokens that are not `)`. That it! There is no actual `)` word in the dictionary.

```python
  def comment(self):
    while next(self.scan()) != ')': pass
```

Scanning is done by walking the currently loaded tokens (from a file or a line at the REPL). Once the tokens are exhausted it reads more (or raise an `EOFError` which prints "done" and falls out of the REPL).

```python
  def scan(self):
    while True:
      for token in self.tokens:
        yield token
      self.read()
```

### `if`/`else`/`then`

Conditional code is expressed as `if ... then` or `if ... else ... then`. Think of `then` as in "Do this, and _then_ continue".

```python
    self.dictionary = {
      'if'   : self.doif,
      'else' : self.doelse,
      'then' : self.dothen,
      ... }
```

For example, we can get the absolute value of a number with the phrase `dup 0 < if -1 * then`. This duplicates the number and tests whether its less than zero and multiplies it by `-1` if so. We could instead get the sign of a number (as `-1` or `1`) with the phrase `0 < if -1 else 1 then`. This again tests whether its less than zero and returns `-1` if so, otherwise `1`.

You may be shocked by the definitions! They merely skip sections of code.

```python
  def doif(self):
    if self.pop() == 0:
      while not next(self.scan()) in ['else', 'then']:
        pass

  def doelse(self):
    while next(self.scan()) != 'then': pass

  def dothen(self): pass
```

This deserves a careful explanation. Given the phrase `0 if ... then`, the condition is false and so `if` skips over everything up to and including the `then`. When the condition is true as in `-1 if ... then`, the `if` doesn't skip anything and the body (`...`) is evaluated up to the `then`, which does nothing (`pass`). When there is an alternative as in `0 if ... else ... then`, the `if` will skip everything up to and including the `else` and that body will then be evaluated (again the trailing `then` does nothing). When the alternative isn't taken as in `-1 if ... else ... then` the `if` will do nothing and evaluation will continue up to the `else`, which then skips everything up to and including the `then`. So these simple definitions work!

### Loops

Counted loops are made with `10 0 do ... loop`. This will count from 0 to 9. The current count may be retrieved with `i`, so for example `10 0 do i 5 * loop` will iterate ten times, pushing 0 to 45 in steps of 5.

```python
    self.dictionary = {
      'do'   : lambda: self.xx_(self.doloop),
      'i'    : lambda: self.push(self.index),
      ... }
```
The current index (`i`) is maintained in state.

```python
    self.index = 0
```

The `doloop` function takes a `start` and `end` (in reverse order) and executes the body of the loop (tokens up to the `loop` token) while updating the `index`. Before doing this, the current `index` is saved in case we're in a nested loop. The current `tokens` are also saved away and both are restored when the loop exits. To evaluate the body, `execute()` is called. Note that `loop` is not a defined word, but instead a delimiting token similar to `)`.

```python
  def doloop(self, end, start):
    savedIndex = self.index # for loop nesting
    savedTokens = self.tokens
    code = list(takewhile(lambda t: t != 'loop', self.scan()))
    self.index = start
    self.tokens = (_ for _ in ())
    while self.index < end:
      self.execute(code)
      self.index += 1
    self.tokens = savedTokens
    self.index = savedIndex

  def execute(self, code):
    self.tokens = chain(code, self.tokens)
    self.evaluate()
```

### Defining Secondary Words

While primitive words are added directly to the dictionary in Python-world, we may add our own definitions to the dictionary from Forth as secondary words; defined in terms of the primitives and/or other secondaries.

For example, `: square dup * ;` adds a new word `square` that when called performs `dup *`, squaring the value on the stack. The colon (`:`) word begins the definition and the semicolon (`;`) delimits the end. It then is indistinguishable from primitive words. `7 square` produces `49.0`.

```python
    self.dictionary = {
      ':'    : self.define,
      ... }
```

The `name` is taken from the token following the colon (`:`). The `code` is everything up to the ending `;` token. Note that `;` is not a defined word, but a delimiting token understood by `:`. A definition is added to the dictionary that when called will `execute(code)`.

```python
  def define(self):
    name = next(self.tokens)
    code = list(takewhile(lambda t: t != ';', self.scan()))
    self.dictionary[name] = (lambda: self.execute(code))
    if not name in self.names: self.names.append([name])
```

Notice that the name is also added to `self.names`. Notice also that once the dictionary has been populated, the names of defined words are collected into `self.names`. The indices of the names are meant to simulate the idea of words having an _address_, which we'll explore in the next section. Note that in a native Forth words have actual memory addresses.

```python
    self.names = [[name] for name in self.dictionary.keys()]
```

### Indirect Calls

We can get the address of a word with `'` and we can `call` words by their address. For example, `7 ' dup call *` is equivalent to just `7 dup *`. The `' dup` gets the address of `dup` and we `call` it.

The reason for allowing this indirection is to enable passing words as arguments to other words; higher order functions! For example, we could define a word that expects two arguments; a value and a word address as `: go swap dup rot call ;`. Expecting a value and an address, this swaps the address out of the way, duplicates the value and rotates the address back and calls the word. We can then further define `: square ' * go ;` and `: double ' + go ;` in terms of this. This idea of a word parameterized by other words is very powerful and we'll see some nice examples later when we build the [turtle graphics](../library/turtle/) library.

Additionally, anonymous sequences of tokens surrounded by `[: ... :]` words may be given an address, similar to lambda functions in other languages. For example, `7 [: square + :] go` will add a number to it's square (`56.0`). Complicated to follow, but the point is that this is a truly powerful (though non-standard) feature.

```python
    self.dictionary = {
      '\''   : lambda: self._x(self.find),
      '[:'   : self.anonymous,
      'call' : lambda: self.x_(self.call),
      ... }
```

Finding a word's address and calling an address is straight forward. Adding anonymous words is done by appending the sequence of tokens up to the ending `:]` token to the `names` list. Interestingly this is actually a list of lists just for this purpose. When an address/index is resolved, the corresponding list is executed. Normally this is a list containing a single word, but in the case of anonymous lambdas its the whole sequence of tokens.

```python
  def find(self):
    name = next(self.tokens)
    i = self.names.index([name])
    return i

  def anonymous(self):
    code = list(takewhile(lambda t: t != ':]', self.scan()))
    self.push(len(self.names))
    self.names.append(code)

  def call(self, i): self.execute(self.names[int(i)])
```

### Miscellaneous

Several miscellaneous words remain.

* `emit` will output to the console (e.g. `104 emit 105 emit` will print "hi")
* `sym` will deconstruct the following token into ASCII characters and length (e.g. `sym hi` results in 105 104 2 on the stack)
* `words` prints the list of available words in currently the dictionary
* `exit` halts the interpreter

```python
    self.dictionary = {
      'emit' : lambda: self.x_(lambda x:stdout.write(chr(int(x)))),
      'sym'  : self.symbol,
      'words': self.words,
      'exit' : lambda: exit(0) }
      ... }
```

Being able to deconstruct tokens into their characters with `sym` will be used when we move away from the Python interpreter to construct the in-memory dictionary when we build a Forth inner interpreter for _hardware_.

```python
  def symbol(self):
    name = next(self.tokens)
    for c in name[::-1]: self.push(ord(c))
    self.push(len(name))

  def words(self):
    for word in self.dictionary:
      print(word, end=' ')
    print()
```

## Next

Next let's use this interpreter to build a [pixels library](../library/pixels/) and a [turtle graphics library](../library/turtle/). Then let's move on to using the interpreter to replace itself by building a new native Forth for [register machine hardware](../hardware/register/).