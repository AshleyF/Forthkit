from sys import stdin, stdout, exit, setrecursionlimit
from itertools import takewhile, chain
import operator, math, struct

setrecursionlimit(100000)

class Forth:
  def __init__(_):
    _.index = 0
    _.variables = {}
    _.memory = [0] * 32 * 1024
    _.stack = []
    _.dictionary = {
      'dup'  : lambda: _.x_xx(lambda x: (x,x)),
      'drop' : lambda: _.x_(lambda _: None),
      'swap' : lambda: _.xx_xx(lambda x,y: (y,x)),
      'over' : lambda: _.xx_xxx(lambda x,y: (x,y,x)),
      'rot'  : lambda: _.xxx_xxx(lambda x,y,z: (y,z,x)),
      '-rot' : lambda: _.xxx_xxx(lambda x,y,z: (z,x,y)),
      '+'    : lambda: _.xx_x(operator.add),
      '-'    : lambda: _.xx_x(operator.sub),
      '*'    : lambda: _.xx_x(operator.mul),
      '/'    : lambda: _.xx_x(operator.div),
      'mod'  : lambda: _.xx_x(operator.mod),
      'acos' : lambda: _.x_x(math.acos),
      'asin' : lambda: _.x_x(math.asin),
      'atan' : lambda: _.x_x(math.atan),
      'cos'  : lambda: _.x_x(math.cos),
      'sin'  : lambda: _.x_x(math.sin),
      'tan'  : lambda: _.x_x(math.tan),
      'int'  : lambda: _.x_x(int),
      'float': lambda: _.x_x(float),
      '>>'   : lambda: _.xx_x(operator.rshift),
      '<<'   : lambda: _.xx_x(operator.lshift),
      '='    : lambda: _.xx_b(operator.eq),
      '<>'   : lambda: _.xx_b(operator.ne),
      '>'    : lambda: _.xx_b(operator.gt),
      '>='   : lambda: _.xx_b(operator.ge),
      '<'    : lambda: _.xx_b(operator.lt),
      '<='   : lambda: _.xx_b(operator.le),
      'and'  : lambda: _.xx_x(operator.and_),
      'or'   : lambda: _.xx_x(operator.or_),
      'xor'  : lambda: _.xx_x(operator.xor),
      'not'  : lambda: _.x_x(operator.inv),
      'm@'   : lambda: _.x_x(lambda x: _.memory[x]),
      'm!'   : lambda: _.xx_(_.memoryStore),
      '@'    : lambda: _.x_x(lambda x: _.variables[x]),
      '!'    : lambda: _.xx_(_.variableStore),
      '.'    : lambda: stdout.write('%f ' % _.pop()),
      '.s'   : lambda: stdout.write('%s ' % _.stack),
      '.m'   : lambda: stdout.write('%s ' % _.memory[_.pop():_.pop()]),
      'emit' : lambda: _.x_(lambda x:stdout.write(unichr(x))),
      'flush': lambda: lambda: stdout.flush(),
      'dump' : _.dump,
      'sym'  : _.symbol,
      '('    : _.comment,
      'if'   : _.doif,
      'else' : _.doelse,
      'then' : _.dothen,
      'do'   : _.doloop,
      'i'    : lambda: _.push(_.index),
      'const': _.constant,
      'var'  : _.variable,
      ':'    : _.define,
      '\''   : lambda: _._x(_.find),
      '['    : _.anonymous,
      'call' : lambda: _.x_(_.call),
      'exit' : lambda: exit(0),
      'debug': lambda: _.debug() }
    _.names = _.dictionary.keys()

  def push(_, x): _.stack.append(x)
  def push2(_, (x, y)): _.push(x); _.push(y)
  def push3(_, (x, y, z)): _.push(x); _.push(y); _.push(z)

  def pop(_):
    if len(_.stack) == 0:
      raise Exception("Stack empty")
    _.stack, val = _.stack[:-1], _.stack[-1]
    return val

  def flip2(_, f, x, y): return f(y, x)
  def flip3(_, f, x, y, z): return f(z, y, x)
  def x_x(_, f): _.push(f(_.pop()))
  def xx_x(_, f): _.push(_.flip2(f, _.pop(), _.pop()))
  def xx_b(_, f): _.push( -1 if _.flip2(f,_.pop(),_.pop()) else 0)
  def xx_xx(_, f): _.push2(_.flip2(f, _.pop(), _.pop()))
  def x_xx(_, f): _.push2(f(_.pop()))
  def xx_xxx(_, f): _.push3(_.flip2(f, _.pop(), _.pop()))
  def xxx_xxx(_, f): _.push3(_.flip3(f, _.pop(), _.pop(), _.pop()))
  def x_(_, f): f(_.pop())
  def _x(_, f): _.push(f())
  def xx_(_, f): _.flip2(f, _.pop(), _.pop())

  def memoryStore(_, val, addr): _.memory[addr] = val
  def variableStore(_, val, addr): _.variables[addr] = val

  def debug(_):
    print 'STACK: %s' % _.stack
    print 'MEMORY: %s' % _.memory[:1000]

  def dump(_):
    with open('boot.bin', 'wb') as f:
      for m in _.memory:
        f.write(struct.pack('h', m))

  def symbol(_):
    name = _.tokens.next()
    for c in name[::-1]: _.push(ord(c))
    _.push(len(name))

  def comment(_):
    while _.scan().next() != ')': pass

  def doif(_):
    term = ['else', 'then']
    if _.pop() == 0:
      while not _.scan().next() in term:
        pass

  def doelse(_):
    while _.scan().next() != 'then': pass

  def dothen(_): pass

  def doloop(_):
    savedIndex = _.index # for loop nesting
    _.index = _.pop()
    end = _.pop()
    code = list(takewhile(lambda t: t != 'loop', _.scan()))
    savedTokens = _.tokens
    _.tokens = (_ for _ in ())
    while _.index < end:
      _.execute(code)
      _.index += 1
    _.tokens = savedTokens
    _.index = savedIndex

  def constant(_):
    name = _.tokens.next()
    val = _.pop()
    _.dictionary[name] = lambda: _.push(val)

  def variable(_):
    name = _.tokens.next()
    index = len(_.variables)
    _.variables[index] = 0
    _.dictionary[name] = lambda: _.push(index)

  def define(_):
    name = _.tokens.next()
    code = list(takewhile(lambda t: t != ';', _.scan()))
    _.dictionary[name] = (lambda: _.execute(code))
    if not name in _.names: _.names.append([name])

  def anonymous(_):
    code = list(takewhile(lambda t: t != ']', _.scan()))
    _.names.append(code)
    _.push(len(_.names) - 1)

  def find(_):
    name = _.tokens.next()
    i = _.names.index([name])
    return i

  def call(_, i): _.execute(_.names[i])

  def input(_):
    for token in raw_input().split(): yield token

  def read(_): _.tokens = _.input()

  def scan(_):
    while True:
      for token in _.tokens:
        yield token
      _.read()

  def number(_, token):
    try:
      return int(token)
    except ValueError:
      return float(token)

  def evaluate(_):
    try:
      while True:
        token = _.tokens.next()
        if token in _.dictionary:
          _.dictionary[token]()
        else:
          try:
            _.push(_.number(token))
          except ValueError:
            raise Exception("%s?" % token)
    except StopIteration: pass

  def execute(_, code):
    _.tokens = chain(code, _.tokens)
    _.evaluate()

forth = Forth()

print "Welcome to PyForth 0.3 REPL"
while True:
  try:
    print '> ',
    stdout.flush()
    forth.read()
    forth.evaluate()
    print 'ok'
  except EOFError: print 'done'; exit()
  except Exception, error: print error