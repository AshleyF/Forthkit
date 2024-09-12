# Interpreter Walkthrough

The core Forth interpreter is in this `Forth` class defined in [interpreter.py](./interpreter.py):

```python
class Forth:
  def __init__(self):
    ...
```

Ulitmately, the `Forth` interpreter will be constructed and driven in a REPL loop by `read()` and `evaluate()`.

```python
forth = Forth()

print("Welcome to PyForth 0.3 REPL")
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

We `read()` whitespace-separated tokens (`.split()`) from the console (`input()`). The input may be typed at the console, or may be piped from files (e.g. `cat foo.4th bar.4th | python ./interpreter.py`). Or `cat` also allows for a `-` which emits files and then relays keyboard input (e.g. to load a file and then proceed interactively `cat foo.4th - | python ./interpreter.py`). The stream of tokens hang off of `tokens`.

```python
  def read(self):
    self.tokens = (token for token in input().split())
```

Evaluation of each token is quite straight forward, making use of the halmark feature of Forth, which is a stack and a dictionary. The `state` will contain arguments waiting to be consumed by _words_, which are functions defined in the `dictionary`.

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

Evaluation peals off tokens one by one and handles them. First we try to find the token in the `dictionary` and call the associated function. If it's not in the dictionary then we try to parse it as a (`float`) number and push it onto the stack. If it's neither a defined word or a literal number then this is an error (a `ValueError` exception is caught a new exception containing `f'{token}?'` is raised and printed by the REPL).

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

That's it for the "inner interpreter"! The rest comes down to adding useful word definitions to the dictionary.

Let's start with some basic math operators.

```python
    self.dictionary = {
      '+'    : lambda: self.xx_x(operator.add),
      '-'    : lambda: self.xx_x(operator.sub),
      '*'    : lambda: self.xx_x(operator.mul),
      '/'    : lambda: self.xx_x(operator.truediv),
      'mod'  : lambda: self.xx_x(operator.mod),
      'acos' : lambda: self.x_x(math.acos),
      'asin' : lambda: self.x_x(math.asin),
      'atan' : lambda: self.x_x(math.atan),
      'cos'  : lambda: self.x_x(math.cos),
      'sin'  : lambda: self.x_x(math.sin),
      'tan'  : lambda: self.x_x(math.tan),
      'floor': lambda: self.x_x(math.floor),
      ... }
```

Notice that binary operations taking two arguments and returning a single result are wrapped in `xx_x(...)` while uniary operations are wrapped in `x_x`. These are helper functions to `pop()` argument and `push()` results.

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

To see the result, we can use `.` or can print the stack with `.s`.

```python
    self.dictionary = {
      '.'    : lambda: stdout.write('%f ' % self.pop()),
      '.s'   : lambda: stdout.write('%s ' % self.stack),
      ... }
```

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

Note that the `xx_b` wrapper function returns a "boolean". We use `-1` and `0` to represent true and false. This is why it pushes `-1 if ... else 0`.

This leads us to the logical operators.

```python
    self.dictionary = {
      'and'  : lambda: self.xx_x(lambda x, y: int(x) & int(y)),
      'or'   : lambda: self.xx_x(lambda x, y: int(x) | int(y)),
      'xor'  : lambda: self.xx_x(lambda x, y: int(x) ^ int(y)),
      'not'  : lambda: self.x_x(lambda x: ~x),
      ... }
```

These appear to be _bitwise_ operators, and they are! They work as _both_ bitwise and locical operators because true (`-1`) is all bits set and false (`0`) is all bits reset. This unification simplifies the vocabulary of words. Compare this to languages like C with `&&`, `||`, `!`, ... vs. `&`, `|`, `~`, ... and most languages don't even have a logical `xor`.






This interpreter is temporary. We will play with this interpreter a bit, but primarily it will be used to build the initial seed image for "hardware" (a VM written in C) from which we will migrate to a more "native" Forth. For building the image, we will use a memory space:

```python
    self.memory = [0] * 32 * 1024
```

We also have a space for named variables and for a loop index:

```python
    self.index = 0
    self.variables = {}
```

We will see later that structures such as the stack, dictionary and variables are merely built in raw memory and that loop indexes generally come from a stack. Here we're making these structures concrete and visible in the interpreter so that we can more clearly see what they represent and so that `memory` remains available exclusively for building the seed image.



    self.index = 0
    self.variables = {}
    self.memory = [0] * 32 * 1024
    self.stack = []
    self.dictionary = {
      'dup'  : lambda: self.x_xx(lambda x: (x,x)),
      'drop' : lambda: self.x_(lambda _: None),
      'swap' : lambda: self.xx_xx(lambda x,y: (y,x)),
      'over' : lambda: self.xx_xxx(lambda x,y: (x,y,x)),
      'rot'  : lambda: self.xxx_xxx(lambda x,y,z: (y,z,x)),
      '-rot' : lambda: self.xxx_xxx(lambda x,y,z: (z,x,y)),
      'm@'   : lambda: self.x_x(lambda x: self.memory[int(x)]),
      'm!'   : lambda: self.xx_(self.memoryStore),
      '@'    : lambda: self.x_x(lambda x: self.variables[int(x)]),
      '!'    : lambda: self.xx_(self.variableStore),
      '.m'   : lambda: stdout.write('%s ' % self.memory[int(self.pop()):int(self.pop())]),
      'emit' : lambda: self.x_(lambda x:stdout.write(chr(int(x)))),
      'dump' : self.dump,
      'sym'  : self.symbol,
      '('    : self.comment,
      'if'   : self.doif,
      'else' : self.doelse,
      'then' : self.dothen,
      'do'   : self.doloop,
      'i'    : lambda: self.push(self.index),
      'const': self.constant,
      'var'  : self.variable,
      ':'    : self.define,
      '\''   : lambda: self._x(self.find),
      '['    : self.anonymous,
      'words': self.words,
      'call' : lambda: self.x_(self.call),
      'exit' : lambda: exit(0) }
    self.names = list(self.dictionary.keys())


  def memoryStore(self, val, addr): self.memory[int(addr)] = int(val)
  def variableStore(self, val, addr): self.variables[addr] = val

  def dump(self):
    with open('image.bin', 'wb') as f:
      for m in self.memory:
        f.write(struct.pack('h', m))

  def symbol(self):
    name = next(self.tokens)
    for c in name[::-1]: self.push(ord(c))
    self.push(len(name))

  def comment(self):
    while next(self.scan()) != ')': pass

  def doif(self):
    term = ['else', 'then']
    if self.pop() == 0:
      while not next(self.scan()) in term:
        pass

  def doelse(self):
    while next(self.scan()) != 'then': pass

  def dothen(self): pass

  def doloop(self):
    savedIndex = self.index # for loop nesting
    self.index = self.pop()
    end = self.pop()
    code = list(takewhile(lambda t: t != 'loop', self.scan()))
    savedTokens = self.tokens
    self.tokens = (_ for _ in ())
    while self.index < end:
      self.execute(code)
      self.index += 1
    self.tokens = savedTokens
    self.index = savedIndex

  def constant(self):
    name = next(self.tokens)
    val = self.pop()
    self.dictionary[name] = lambda: self.push(val)

  def variable(self):
    name = next(self.tokens)
    index = len(self.variables)
    self.variables[index] = 0
    self.dictionary[name] = lambda: self.push(index)

  def define(self):
    name = next(self.tokens)
    code = list(takewhile(lambda t: t != ';', self.scan()))
    self.dictionary[name] = (lambda: self.execute(code))
    if not name in self.names: self.names.append([name])

  def anonymous(self):
    code = list(takewhile(lambda t: t != ']', self.scan()))
    self.names.append(code)
    self.push(len(self.names) - 1)

  def words(self):
    for word in self.dictionary:
      print(word, end=' ')
    print()

  def find(self):
    name = next(self.tokens)
    i = self.names.index([name])
    return i

  def call(self, i): self.execute(self.names[i])

  def scan(self):
    while True:
      for token in self.tokens:
        yield token
      self.read()

  def execute(self, code):
    self.tokens = chain(code, self.tokens)
    self.evaluate()