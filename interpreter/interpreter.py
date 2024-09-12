from sys import stdin, stdout, exit, setrecursionlimit
from itertools import takewhile, chain
import operator, math, struct

setrecursionlimit(100000)

class Forth:
  def __init__(self):
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
      'int'  : lambda: self.x_x(int),
      'float': lambda: self.x_x(float),
      '>>'   : lambda: self.xx_x(operator.rshift),
      '<<'   : lambda: self.xx_x(operator.lshift),
      '='    : lambda: self.xx_b(operator.eq),
      '<>'   : lambda: self.xx_b(operator.ne),
      '>'    : lambda: self.xx_b(operator.gt),
      '>='   : lambda: self.xx_b(operator.ge),
      '<'    : lambda: self.xx_b(operator.lt),
      '<='   : lambda: self.xx_b(operator.le),
      'and'  : lambda: self.xx_x(operator.and_),
      'or'   : lambda: self.xx_x(operator.or_),
      'xor'  : lambda: self.xx_x(operator.xor),
      'not'  : lambda: self.x_x(operator.inv),
      'm@'   : lambda: self.x_x(lambda x: self.memory[int(x)]),
      'm!'   : lambda: self.xx_(self.memoryStore),
      '@'    : lambda: self.x_x(lambda x: self.variables[x]),
      '!'    : lambda: self.xx_(self.variableStore),
      '.'    : lambda: stdout.write('%f ' % self.pop()),
      '.s'   : lambda: stdout.write('%s ' % self.stack),
      '.m'   : lambda: stdout.write('%s ' % self.memory[self.pop():self.pop()]),
      'emit' : lambda: self.x_(lambda x:stdout.write(chr(x))),
      'flush': lambda: lambda: stdout.flush(),
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

  def push(self, x): self.stack.append(x)
  def push2(self, xy):
    x, y = xy
    self.push(x)
    self.push(y)
  def push3(self, xyz):
    x, y, z = xyz
    self.push(x)
    self.push(y)
    self.push(z)

  def pop(self):
    if len(self.stack) == 0:
      raise Exception("Stack empty")
    self.stack, val = self.stack[:-1], self.stack[-1]
    return val

  def flip2(self, f, x, y): return f(y, x)
  def flip3(self, f, x, y, z): return f(z, y, x)
  def x_x(self, f): self.push(f(self.pop()))
  def xx_x(self, f): self.push(self.flip2(f, self.pop(), self.pop()))
  def xx_b(self, f): self.push( -1 if self.flip2(f,self.pop(),self.pop()) else 0)
  def xx_xx(self, f): self.push2(self.flip2(f, self.pop(), self.pop()))
  def x_xx(self, f): self.push2(f(self.pop()))
  def xx_xxx(self, f): self.push3(self.flip2(f, self.pop(), self.pop()))
  def xxx_xxx(self, f): self.push3(self.flip3(f, self.pop(), self.pop(), self.pop()))
  def x_(self, f): f(self.pop())
  def _x(self, f): self.push(f())
  def xx_(self, f): self.flip2(f, self.pop(), self.pop())

  def memoryStore(self, val, addr): self.memory[int(addr)] = val
  def variableStore(self, val, addr): self.variables[addr] = val

  def dump(self):
    with open('boot.bin', 'wb') as f:
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

  def input(self):
    for token in input().split(): yield token

  def read(self): self.tokens = self.input()

  def scan(self):
    while True:
      for token in self.tokens:
        yield token
      self.read()

  def number(self, token):
    try:
      return int(token)
    except ValueError:
      return float(token)

  def evaluate(self):
    try:
      while True:
        token = next(self.tokens)
        #print(token)
        if token in self.dictionary:
          self.dictionary[token]()
        else:
          try:
            self.push(self.number(token))
          except ValueError:
            raise Exception("%s?" % token)
    except StopIteration: pass

  def execute(self, code):
    self.tokens = chain(code, self.tokens)
    self.evaluate()

forth = Forth()

print("Welcome to PyForth 0.3 REPL")
while True:
  try:
    print('> ', end='')
    stdout.flush()
    forth.read()
    forth.evaluate()
    print('ok')
  except EOFError:
    print('done')
    exit()
  except Exception as error: print(error)