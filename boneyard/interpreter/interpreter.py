from sys import stdin, stdout, exit, setrecursionlimit
from itertools import takewhile, chain
import operator, math, struct

setrecursionlimit(100000)

class Forth:
  def __init__(self):
    self.index = 0
    self.variables = []
    self.memory = bytearray(64 * 1024)
    self.stack = []
    self.dictionary = {
      '.'        : lambda: print(self.pop()),
      '.s'       : lambda: print(self.stack),
      'nand'     : lambda: self.xx_x(lambda x,y: ~(int(x) & int(y))),
      '+'        : lambda: self.xx_x(operator.add),
      '-'        : lambda: self.xx_x(operator.sub),
      '*'        : lambda: self.xx_x(operator.mul),
      '/'        : lambda: self.xx_x(operator.truediv),
      'mod'      : lambda: self.xx_x(operator.mod),
      'cos'      : lambda: self.x_x(math.cos),
      'sin'      : lambda: self.x_x(math.sin),
      'tan'      : lambda: self.x_x(math.tan),
      'acos'     : lambda: self.x_x(math.acos),
      'asin'     : lambda: self.x_x(math.asin),
      'atan'     : lambda: self.x_x(math.atan),
      'floor'    : lambda: self.x_x(math.floor),
      '='        : lambda: self.xx_b(operator.eq),
      '<>'       : lambda: self.xx_b(operator.ne),
      '>'        : lambda: self.xx_b(operator.gt),
      '>='       : lambda: self.xx_b(operator.ge),
      '<'        : lambda: self.xx_b(operator.lt),
      '<='       : lambda: self.xx_b(operator.le),
      'lshift'   : lambda: self.xx_x(lambda x,y: int(x) << int(y)),
      'rshift'   : lambda: self.xx_x(lambda x,y: int(x) >> int(y)),
      'pick'     : lambda: self.x_(lambda x: self.stack.append(self.stack[int(-x - 1)])),
      'roll'     : lambda: self.x_(lambda x: (self.stack.append(self.stack[int(-x - 1)]), self.stack.pop(int(-x - 2)))),
      'drop'     : lambda: self.x_(lambda _: None),
      'variable' : self.variable,
      'constant' : self.constant,
      '@'        : lambda: self.x_x(self.fetch),
      '!'        : lambda: self.xx_(self.store),
      'm!'       : lambda: self.xx_(self.store),
      'c@'       : lambda: self.x_x(lambda x: self.memory[int(x)]),
      'c!'       : lambda: self.xx_(self.memoryStoreByte),
      'b@'       : lambda: self.x_x(lambda x: self.memory[int(x)]),
      'b!'       : lambda: self.xx_(self.memoryStoreByte),
      'write'    : lambda: self.xxx_(self.write),
      '('        : self.comment,
      'if'       : self.doif,
      'else'     : self.doelse,
      'then'     : self.dothen,
      'do'       : lambda: self.xx_(self.doloop),
      'i'        : lambda: self.push(self.index),
      ':'        : self.define,
      '\''       : lambda: self._x(self.find),
      '[:'       : self.anonymous,
      'call'     : lambda: self.x_(self.call),
      'emit'     : lambda: self.x_(lambda x: stdout.write(chr(int(x)))),
      'sym'      : self.symbol,
      'words'    : self.words,
      'halt'     : lambda: exit(0) }
    self.names = [[name] for name in self.dictionary.keys()]

  def pop(self):
    if len(self.stack) == 0:
      raise Exception("Stack empty")
    self.stack, val = self.stack[:-1], self.stack[-1]
    return val

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
  def xxx_(self, f): self.flip3(f, self.pop(), self.pop(), self.pop())

  def fetch(self, addr):
    if addr >= 0: return self.memory[int(addr)] | self.memory[int(addr + 1)] << 8
    else: return self.variables[int(-addr) - 1]

  def store(self, val, addr):
    if addr >= 0:
      self.memory[int(addr)] = int(val) & 0xFF
      self.memory[int(addr) + 1] = (int(val) >> 8) & 0xFF
    else: self.variables[-addr - 1] = val

  def memoryStoreByte(self, val, addr): self.memory[int(addr)] = int(val) & 0xFF

  def variable(self):
    name = next(self.tokens)
    index = -len(self.variables) - 1
    self.variables.append(0)
    self.dictionary[name] = lambda: self.push(index)

  def constant(self):
    name = next(self.tokens)
    val = self.pop()
    self.dictionary[name] = lambda: self.push(val)

  def write(self, block, size, address):
    with open(f'block{int(block)}.bin', 'wb') as f:
      for m in self.memory[int(address):int(address + size)]:
        f.write(struct.pack('B', m))

  def scan(self):
    while True:
      for token in self.tokens:
        yield token
      self.read()

  def comment(self):
    while next(self.scan()) != ')': pass

  def doif(self):
    if self.pop() == 0:
      while not next(self.scan()) in ['else', 'then']:
        pass

  def doelse(self):
    while next(self.scan()) != 'then': pass

  def dothen(self): pass

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

  def define(self):
    name = next(self.tokens)
    code = list(map(lambda w: name if w == 'recurse' else w, takewhile(lambda t: t != ';', self.scan())))
    self.dictionary[name] = (lambda: self.execute(code))
    if not name in self.names: self.names.append([name])

  def find(self):
    name = next(self.tokens)
    i = self.names.index([name])
    return i

  def anonymous(self):
    code = list(takewhile(lambda t: t != ':]', self.scan()))
    self.push(len(self.names))
    self.names.append(code)

  def call(self, i): self.execute(self.names[int(i)])

  def symbol(self):
    name = next(self.tokens)
    for c in name[::-1]: self.push(ord(c))
    self.push(len(name))

  def words(self):
    for word in self.dictionary:
      print(word, end=' ')
    print()

  def read(self):
    self.tokens = (token for token in input().split())

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

forth = Forth()

print("Welcome to PyForth REPL")
while True:
  try:
    forth.read()
    forth.evaluate()
  except EOFError:
    print('done')
    exit()
  except Exception as error: print(error)